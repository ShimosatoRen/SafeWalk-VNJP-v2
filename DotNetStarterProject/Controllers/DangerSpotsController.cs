using DotNetStarterProject.Data;
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
    public async Task<IActionResult> Index(string? category, string? searchString)
    {
        var allSpots = await _context.DangerSpots.ToListAsync();
        var groupCounts = allSpots.GroupBy(s => new { 
            Lat = Math.Round(s.Latitude, 4), 
            Lng = Math.Round(s.Longitude, 4) 
        }).ToDictionary(g => g.Key, g => g.Count());

        var query = _context.DangerSpots.Include(d => d.User).AsQueryable();

        if (!string.IsNullOrEmpty(category))
        {
            query = query.Where(d => d.Category == category);
        }

        if (!string.IsNullOrEmpty(searchString))
        {
            query = query.Where(d => d.Title.Contains(searchString) || d.Description.Contains(searchString));
        }

        var dangerSpots = await query.OrderByDescending(d => d.CreatedAt).ToListAsync();

        foreach (var spot in dangerSpots)
        {
            spot.ReportCount = groupCounts[new { 
                Lat = Math.Round(spot.Latitude, 4), 
                Lng = Math.Round(spot.Longitude, 4) 
            }];
        }

        ViewData["CurrentCategory"] = category;
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
    public async Task<IActionResult> Create([Bind("Title,Description,Category,Latitude,Longitude")] DangerSpot dangerSpot, IFormFile? imageFile)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return Challenge();
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

            if (string.IsNullOrEmpty(dangerSpot.Title) || string.IsNullOrEmpty(dangerSpot.Description) || string.IsNullOrEmpty(dangerSpot.Category))
            {
                ModelState.AddModelError("", "タイトル、詳細、カテゴリは必須入力項目です。");
                return View(dangerSpot);
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
    public async Task<IActionResult> Edit(long id, [Bind("Id,Title,Description,Category,Latitude,Longitude,UserId,CreatedAt,ImagePath")] DangerSpot dangerSpot, IFormFile? imageFile)
    {
        if (id != dangerSpot.Id)
        {
            return NotFound();
        }

        var user = await _userManager.GetUserAsync(User);
        if (user == null || dangerSpot.UserId != user.Id)
        {
            return Forbid();
        }

        try
        {
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

                // Optionally delete old image here
                dangerSpot.ImagePath = "/uploads/" + uniqueFileName;
            }

            if (string.IsNullOrEmpty(dangerSpot.Title) || string.IsNullOrEmpty(dangerSpot.Description) || string.IsNullOrEmpty(dangerSpot.Category))
            {
                ModelState.AddModelError("", "タイトル、詳細、カテゴリは必須入力項目です。");
                return View(dangerSpot);
            }

            dangerSpot.UpdatedAt = DateTime.UtcNow;
            _context.Update(dangerSpot);
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
