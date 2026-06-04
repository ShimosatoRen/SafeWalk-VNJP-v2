using System.ComponentModel.DataAnnotations;

namespace DotNetStarterProject.Models;

public sealed class RegisterViewModel
{
    [Required(ErrorMessage = "ユーザー名を入力してください。")]
    [Display(Name = "ユーザー名")]
    public string UserName { get; init; } = string.Empty;

    [Required(ErrorMessage = "メールアドレスを入力してください。")]
    [EmailAddress(ErrorMessage = "有効なメールアドレスを入力してください。")]
    [Display(Name = "メールアドレス")]
    public string Email { get; init; } = string.Empty;

    [Required(ErrorMessage = "パスワードを入力してください。")]
    [DataType(DataType.Password)]
    [Display(Name = "パスワード")]
    public string Password { get; init; } = string.Empty;

    [Required(ErrorMessage = "パスワード（確認）を入力してください。")]
    [DataType(DataType.Password)]
    [Display(Name = "パスワード（確認）")]
    [Compare("Password", ErrorMessage = "パスワードが一致しません。")]
    public string ConfirmPassword { get; init; } = string.Empty;
}

public sealed class LoginViewModel
{
    [Required(ErrorMessage = "メールアドレスを入力してください。")]
    [EmailAddress(ErrorMessage = "有効なメールアドレスを入力してください。")]
    [Display(Name = "メールアドレス")]
    public string Email { get; init; } = string.Empty;

    [Required(ErrorMessage = "パスワードを入力してください。")]
    [DataType(DataType.Password)]
    [Display(Name = "パスワード")]
    public string Password { get; init; } = string.Empty;

    public bool RememberMe { get; init; }

    public string? ReturnUrl { get; init; }
}
