using System.ComponentModel.DataAnnotations;

namespace DotNetStarterProject.Models;

public sealed class RegisterViewModel
{
    [Required]
    [Display(Name = "ユーザー名")]
    public string UserName { get; init; } = string.Empty;

    [Required]
    [EmailAddress]
    [Display(Name = "メールアドレス")]
    public string Email { get; init; } = string.Empty;

    [Required]
    [DataType(DataType.Password)]
    [Display(Name = "パスワード")]
    public string Password { get; init; } = string.Empty;

    [Required]
    [DataType(DataType.Password)]
    [Display(Name = "パスワード（確認）")]
    [Compare("Password", ErrorMessage = "パスワードが一致しません。")]
    public string ConfirmPassword { get; init; } = string.Empty;
}

public sealed class LoginViewModel
{
    [Required]
    [EmailAddress]
    public string Email { get; init; } = string.Empty;

    [Required]
    [DataType(DataType.Password)]
    public string Password { get; init; } = string.Empty;

    public bool RememberMe { get; init; }

    public string? ReturnUrl { get; init; }
}
