using Microsoft.AspNetCore.Identity;

namespace DotNetStarterProject.Models;

public sealed class ApplicationUser : IdentityUser
{
    public DateTime? AdFreeUntil { get; set; }
    public int Points { get; set; }
    public DateTime RegistrationDate { get; set; } = DateTime.UtcNow;
    public ICollection<DangerSpot> DangerSpots { get; set; } = new List<DangerSpot>();
}
