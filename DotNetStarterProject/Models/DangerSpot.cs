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

    [Required(ErrorMessage = "タイトルを入力してください。")]
    [MaxLength(100, ErrorMessage = "タイトルは100文字以内で入力してください。")]
    [Display(Name = "タイトル")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "詳細な状況を入力してください。")]
    [MaxLength(1000, ErrorMessage = "詳細は1000文字以内で入力してください。")]
    [Display(Name = "詳細な状況")]
    public string Description { get; set; } = string.Empty;

    [Required(ErrorMessage = "カテゴリを選択してください。")]
    [MaxLength(50)]
    [Display(Name = "カテゴリ")]
    public string Category { get; set; } = string.Empty;

    [Required(ErrorMessage = "緯度を指定してください。マップをクリックして選択できます。")]
    [Column(TypeName = "decimal(10, 7)")]
    [Display(Name = "緯度")]
    public decimal Latitude { get; set; }

    [Required(ErrorMessage = "経度を指定してください。マップをクリックして選択できます。")]
    [Column(TypeName = "decimal(10, 7)")]
    [Display(Name = "経度")]
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
