using Microsoft.AspNetCore.Identity;

namespace DotNetStarterProject.Models;

public sealed class ApplicationUser : IdentityUser
{
    public DateTime? AdFreeUntil { get; set; }
    public int Points { get; set; }
}
