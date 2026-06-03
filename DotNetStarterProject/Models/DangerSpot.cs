using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DotNetStarterProject.Models;

public enum DangerSpotStatus
{
    Open,
    Confirmed,
    Resolved
}

public class DangerSpot
{
    public long Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [MaxLength(1000)]
    public string Description { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string Category { get; set; } = string.Empty;

    [Required]
    [Column(TypeName = "decimal(10, 7)")]
    public decimal Latitude { get; set; }

    [Required]
    [Column(TypeName = "decimal(10, 7)")]
    public decimal Longitude { get; set; }

    [MaxLength(255)]
    public string? ImagePath { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    [Required]
    public string UserId { get; set; } = string.Empty;

    [ForeignKey("UserId")]
    public ApplicationUser? User { get; set; }

    public DangerSpotStatus Status { get; set; } = DangerSpotStatus.Open;

    [NotMapped]
    public int ReportCount { get; set; }
}
