using DotNetStarterProject.Data;
using DotNetStarterProject.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DotNetStarterProject.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public AdminController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var userCount = await _context.Users.CountAsync();
            var spotCount = await _context.DangerSpots.CountAsync();
            var openSpots = await _context.DangerSpots.CountAsync(s => s.Status == DangerSpotStatus.Open);

            ViewBag.UserCount = userCount;
            ViewBag.SpotCount = spotCount;
            ViewBag.OpenSpots = openSpots;

            return View();
        }

        public async Task<IActionResult> Users()
        {
            var users = await _context.Users.OrderByDescending(u => u.RegistrationDate).ToListAsync();
            return View(users);
        }

        public async Task<IActionResult> DangerSpots()
        {
            var spots = await _context.DangerSpots
                .Include(s => s.User)
                .OrderByDescending(s => s.CreatedAt)
                .ToListAsync();
            return View(spots);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateStatus(long id, DangerSpotStatus status)
        {
            var spot = await _context.DangerSpots.FindAsync(id);
            if (spot != null)
            {
                spot.Status = status;
                spot.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "ステータスを更新しました。";
            }
            return RedirectToAction(nameof(DangerSpots));
        }

        [HttpPost]
        public async Task<IActionResult> ToggleAdmin(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user != null && user.UserName != "admin") // 固定のadminユーザーは変更不可
            {
                if (await _userManager.IsInRoleAsync(user, "Admin"))
                {
                    await _userManager.RemoveFromRoleAsync(user, "Admin");
                    TempData["SuccessMessage"] = $"{user.UserName} の管理者権限を削除しました。";
                }
                else
                {
                    await _userManager.AddToRoleAsync(user, "Admin");
                    TempData["SuccessMessage"] = $"{user.UserName} に管理者権限を付与しました。";
                }
            }
            return RedirectToAction(nameof(Users));
        }

        [HttpPost]
        public async Task<IActionResult> AdjustPoints(string userId, int points)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user != null)
            {
                user.Points = points;
                var result = await _userManager.UpdateAsync(user);
                if (result.Succeeded)
                {
                    TempData["SuccessMessage"] = $"{user.UserName} のポイントを {points}pt に変更しました。";
                }
                else
                {
                    TempData["ErrorMessage"] = "ポイントの更新に失敗しました。";
                }
            }
            return RedirectToAction(nameof(Users));
        }

        [HttpPost]
        public async Task<IActionResult> DeleteUser(string userId)
        {
            var user = await _userManager.Users
                .Include(u => u.DangerSpots)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user != null && user.UserName != "admin")
            {
                // 物理ファイルの削除
                foreach (var spot in user.DangerSpots)
                {
                    if (!string.IsNullOrEmpty(spot.ImagePath))
                    {
                        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", spot.ImagePath.TrimStart('/'));
                        if (System.IO.File.Exists(filePath))
                        {
                            System.IO.File.Delete(filePath);
                        }
                    }
                }

                var result = await _userManager.DeleteAsync(user);
                if (result.Succeeded)
                {
                    TempData["SuccessMessage"] = $"ユーザー {user.UserName} と関連データを削除しました。";
                }
                else
                {
                    TempData["ErrorMessage"] = "ユーザーの削除に失敗しました。";
                }
            }
            return RedirectToAction(nameof(Users));
        }

        [HttpPost]
        public async Task<IActionResult> DeleteSpot(long id)
        {
            var spot = await _context.DangerSpots.FindAsync(id);
            if (spot != null)
            {
                _context.DangerSpots.Remove(spot);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "投稿を削除しました。";
            }
            return RedirectToAction(nameof(DangerSpots));
        }
    }
}
