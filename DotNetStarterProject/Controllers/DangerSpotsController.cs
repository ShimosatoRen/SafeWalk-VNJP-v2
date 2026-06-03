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
    public async Task<IActionResult> Index(string? category, int? dangerLevel, string? searchString)
    {
        var query = _context.DangerSpots.Include(d => d.User).AsQueryable();

        if (!string.IsNullOrEmpty(category))
        {
            query = query.Where(d => d.Category == category);
        }

        if (dangerLevel.HasValue)
        {
            query = query.Where(d => d.DangerLevel == dangerLevel.Value);
        }

        if (!string.IsNullOrEmpty(searchString))
        {
            query = query.Where(d => d.Title.Contains(searchString) || d.Description.Contains(searchString));
        }

        var dangerSpots = await query.OrderByDescending(d => d.CreatedAt).ToListAsync();

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

        return View(dangerSpot);
    }

    // GET: DangerSpots/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: DangerSpots/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Title,Description,Category,DangerLevel,Latitude,Longitude")] DangerSpot dangerSpot, IFormFile? imageFile)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return Challenge();
        }

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

        // We don't use ModelState.IsValid here because we are setting UserId and CreatedAt manually
        // and [Bind] might miss some required fields or include navigation properties.
        // For simplicity in this starter, we'll do basic checks.
        
        if (string.IsNullOrEmpty(dangerSpot.Title) || string.IsNullOrEmpty(dangerSpot.Description) || string.IsNullOrEmpty(dangerSpot.Category))
        {
            ModelState.AddModelError("", "Title, Description, and Category are required.");
            return View(dangerSpot);
        }

        _context.Add(dangerSpot);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
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
    public async Task<IActionResult> Edit(long id, [Bind("Id,Title,Description,Category,DangerLevel,Latitude,Longitude,UserId,CreatedAt,ImagePath")] DangerSpot dangerSpot, IFormFile? imageFile)
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
            ModelState.AddModelError("", "Title, Description, and Category are required.");
            return View(dangerSpot);
        }

        try
        {
            dangerSpot.UpdatedAt = DateTime.UtcNow;
            _context.Update(dangerSpot);
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!_context.DangerSpots.Any(e => e.Id == dangerSpot.Id))
            {
                return NotFound();
            }
            else
            {
                throw;
            }
        }
        return RedirectToAction(nameof(Index));
    }

    // POST: DangerSpots/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(long id)
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
        }

        return RedirectToAction(nameof(Index));
    }
}
