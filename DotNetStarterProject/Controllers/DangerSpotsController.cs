using DotNetStarterProject.Data;
using DotNetStarterProject.Helpers;
using DotNetStarterProject.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DotNetStarterProject.Controllers;

[Authorize]
public class DangerSpotsController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IWebHostEnvironment _hostEnvironment;

    public DangerSpotsController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        IWebHostEnvironment hostEnvironment)
    {
        _context = context;
        _userManager = userManager;
        _hostEnvironment = hostEnvironment;
    }

    // GET: DangerSpots
    public async Task<IActionResult> Index(string? category, int? dangerLevel, string? searchString)
    {
        var allSpots = await _context.DangerSpots.ToListAsync();
        var groupCounts = allSpots.GroupBy(s => new { 
            Lat = Math.Round(s.Latitude, 4), 
            Lng = Math.Round(s.Longitude, 4) 
        }).ToDictionary(g => g.Key, g => g.Count());

        var query = _context.DangerSpots.Include(d => d.User).AsQueryable();

        if (!string.IsNullOrEmpty(category))
        {
            query = query.Where(d => d.Category.Contains(category));
        }

        if (!string.IsNullOrEmpty(searchString))
        {
            query = query.Where(d => d.Title.Contains(searchString) || d.Description.Contains(searchString));
        }

        var dangerSpots = await query.OrderByDescending(d => d.CreatedAt).ToListAsync();

        // カテゴリベースのレベルと報告数を計算
        foreach (var spot in dangerSpots)
        {
            spot.ReportCount = groupCounts[new { 
                Lat = Math.Round(spot.Latitude, 4), 
                Lng = Math.Round(spot.Longitude, 4) 
            }];
            spot.Level = DangerLevelHelper.CalculateLevel(spot.Category);
        }

        // 危険レベルでフィルタリング
        if (dangerLevel.HasValue)
        {
            dangerSpots = dangerSpots.Where(s => s.Level == dangerLevel.Value).ToList();
        }

        ViewData["CurrentCategory"] = category;
        ViewData["CurrentDangerLevel"] = dangerLevel;
        ViewData["CurrentFilter"] = searchString;

        return View(dangerSpots);
    }

    // GET: DangerSpots/Details/5
    public async Task<IActionResult> Details(long? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var dangerSpot = await _context.DangerSpots
            .Include(d => d.User)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (dangerSpot == null)
        {
            return NotFound();
        }

        dangerSpot.ReportCount = await _context.DangerSpots
            .CountAsync(s => Math.Round(s.Latitude, 4) == Math.Round(dangerSpot.Latitude, 4) && 
                            Math.Round(s.Longitude, 4) == Math.Round(dangerSpot.Longitude, 4));
        
        dangerSpot.Level = DangerLevelHelper.CalculateLevel(dangerSpot.Category);

        return View(dangerSpot);
    }

    // GET: DangerSpots/Map
    [AllowAnonymous]
    public async Task<IActionResult> Map()
    {
        var dangerSpots = await _context.DangerSpots.Include(d => d.User).ToListAsync();
        
        var groupCounts = dangerSpots.GroupBy(s => new { 
            Lat = Math.Round(s.Latitude, 4), 
            Lng = Math.Round(s.Longitude, 4) 
        }).ToDictionary(g => g.Key, g => g.Count());

        foreach (var spot in dangerSpots)
        {
            spot.ReportCount = groupCounts[new { 
                Lat = Math.Round(spot.Latitude, 4), 
                Lng = Math.Round(spot.Longitude, 4) 
            }];
        }

        return View(dangerSpots);
    }

    // GET: DangerSpots/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: DangerSpots/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        [Bind("Title,Description,Latitude,Longitude")] DangerSpot dangerSpot,
        string[] SelectedCategories,
        IFormFile? imageFile)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return Challenge();
        }

        // カテゴリの設定
        dangerSpot.Category = SelectedCategories != null && SelectedCategories.Length > 0 
            ? string.Join(",", SelectedCategories) 
            : string.Empty;
        
        // バリデーションの調整
        ModelState.Remove("Category");
        ModelState.Remove("UserId");
        ModelState.Remove("User");

        if (string.IsNullOrEmpty(dangerSpot.Category))
        {
            ModelState.AddModelError("Category", "カテゴリを選択してください。");
        }

        if (!ModelState.IsValid)
        {
            return View(dangerSpot);
        }

        try
        {
            dangerSpot.UserId = user.Id;
            dangerSpot.CreatedAt = DateTime.UtcNow;

            if (imageFile != null && imageFile.Length > 0)
            {
                string uploadsFolder = Path.Combine(_hostEnvironment.WebRootPath, "uploads");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                string uniqueFileName = Guid.NewGuid().ToString() + "_" + imageFile.FileName;
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await imageFile.CopyToAsync(fileStream);
                }

                dangerSpot.ImagePath = "/uploads/" + uniqueFileName;
            }

            _context.Add(dangerSpot);
            
            // Grant 24-hour ad-free reward
            user.AdFreeUntil = DateTime.UtcNow.AddHours(24);
            
            // Grant 10 points
            user.Points += 10;
            
            await _userManager.UpdateAsync(user);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "投稿が完了しました！報酬として10ポイントと24時間の広告非表示特典が付与されました。";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception)
        {
            // Log the exception
            ModelState.AddModelError("", "投稿の保存中にエラーが発生しました。入力内容を確認し、再度お試しください。");
            return View(dangerSpot);
        }
    }

    // POST: DangerSpots/ChangeStatus/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangeStatus(long id, DangerSpotStatus status)
    {
        var dangerSpot = await _context.DangerSpots.FindAsync(id);
        if (dangerSpot == null)
        {
            return NotFound();
        }

        var user = await _userManager.GetUserAsync(User);
        if (user == null || dangerSpot.UserId != user.Id)
        {
            return Forbid();
        }

        dangerSpot.Status = status;
        dangerSpot.UpdatedAt = DateTime.UtcNow;
        _context.Update(dangerSpot);
        await _context.SaveChangesAsync();
        
        return RedirectToAction(nameof(Details), new { id = id });
    }

    // GET: DangerSpots/Edit/5
    public async Task<IActionResult> Edit(long? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var dangerSpot = await _context.DangerSpots.FindAsync(id);
        if (dangerSpot == null)
        {
            return NotFound();
        }

        var user = await _userManager.GetUserAsync(User);
        if (user == null || dangerSpot.UserId != user.Id)
        {
            return Forbid();
        }

        return View(dangerSpot);
    }

    // POST: DangerSpots/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        long id,
        [Bind("Id,Title,Description,Latitude,Longitude")] DangerSpot dangerSpot,
        string[] SelectedCategories,
        IFormFile? imageFile)
    {
        if (id != dangerSpot.Id)
        {
            return NotFound();
        }

        // 既存データの取得（追跡あり）
        var spotToUpdate = await _context.DangerSpots.FirstOrDefaultAsync(s => s.Id == id);
        if (spotToUpdate == null)
        {
            return NotFound();
        }

        // 所有者確認
        var user = await _userManager.GetUserAsync(User);
        if (user == null || spotToUpdate.UserId != user.Id)
        {
            return Forbid();
        }

        // カテゴリの処理
        var joinedCategories = SelectedCategories != null && SelectedCategories.Length > 0 
            ? string.Join(",", SelectedCategories) 
            : string.Empty;
        
        // バリデーションの調整
        ModelState.Remove("Category");
        ModelState.Remove("UserId");
        ModelState.Remove("User");

        if (string.IsNullOrEmpty(joinedCategories))
        {
            ModelState.AddModelError("Category", "カテゴリを選択してください。");
        }

        if (!ModelState.IsValid)
        {
            // バリデーションエラー時は元の情報を一部戻して表示
            dangerSpot.Category = joinedCategories;
            // UserIdやCreatedAtなどはDBにあるので、必要なら補完する（Viewでの表示用）
            dangerSpot.UserId = spotToUpdate.UserId;
            dangerSpot.CreatedAt = spotToUpdate.CreatedAt;
            dangerSpot.ImagePath = spotToUpdate.ImagePath;
            return View(dangerSpot);
        }

        try
        {
            // 各フィールドの更新
            spotToUpdate.Title = dangerSpot.Title;
            spotToUpdate.Description = dangerSpot.Description;
            spotToUpdate.Latitude = dangerSpot.Latitude;
            spotToUpdate.Longitude = dangerSpot.Longitude;
            spotToUpdate.Category = joinedCategories;
            spotToUpdate.UpdatedAt = DateTime.UtcNow;

            if (imageFile != null && imageFile.Length > 0)
            {
                string uploadsFolder = Path.Combine(_hostEnvironment.WebRootPath, "uploads");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                string uniqueFileName = Guid.NewGuid().ToString() + "_" + imageFile.FileName;
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await imageFile.CopyToAsync(fileStream);
                }

                spotToUpdate.ImagePath = "/uploads/" + uniqueFileName;
            }

            await _context.SaveChangesAsync();
            
            TempData["SuccessMessage"] = "投稿を更新しました。";
            return RedirectToAction(nameof(Index));
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!_context.DangerSpots.Any(e => e.Id == dangerSpot.Id))
            {
                return NotFound();
            }
            else
            {
                ModelState.AddModelError("", "他のユーザーによって更新された可能性があります。再度読み込み直してください。");
            }
        }
        catch (Exception)
        {
            ModelState.AddModelError("", "更新中にエラーが発生しました。");
        }
        return View(dangerSpot);
    }

    // POST: DangerSpots/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(long id)
    {
        try
        {
            var dangerSpot = await _context.DangerSpots.FindAsync(id);
            if (dangerSpot != null)
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null || dangerSpot.UserId != user.Id)
                {
                    return Forbid();
                }

                _context.DangerSpots.Remove(dangerSpot);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "投稿を削除しました。";
            }
        }
        catch (Exception)
        {
            TempData["ErrorMessage"] = "削除中にエラーが発生しました。";
        }

        return RedirectToAction(nameof(Index));
    }
}
