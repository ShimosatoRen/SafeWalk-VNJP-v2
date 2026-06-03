using DotNetStarterProject.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using System.Diagnostics;

namespace DotNetStarterProject.Controllers
{
    public class HomeController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly Data.ApplicationDbContext _context;

        public HomeController(IConfiguration configuration, Data.ApplicationDbContext context)
        {
            _configuration = configuration;
            _context = context;
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var dangerSpots = await _context.DangerSpots.Include(d => d.User).ToListAsync();
            
            // Group by coordinates (rounded to 4 decimals, approx 11m) to count reports per area
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

        [Authorize]
        public IActionResult Privacy()
        {
            return View();
        }

        [AllowAnonymous]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
