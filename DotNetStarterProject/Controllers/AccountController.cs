using DotNetStarterProject.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace DotNetStarterProject.Controllers;

public sealed class AccountController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;

    public AccountController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
    }

    [HttpGet]
    public IActionResult Register()
    {
        return View(new RegisterViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            var user = new ApplicationUser
            {
                UserName = model.UserName,
                Email = model.Email
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (result.Succeeded)
            {
                await _signInManager.SignInAsync(user, isPersistent: false);
                TempData["SuccessMessage"] = "ユーザー登録が完了しました。ようこそ SafeWalk VNJP へ！";
                return RedirectToAction("Index", "Home");
            }

            AddIdentityErrors(result);
        }
        catch (Exception)
        {
            // Log the exception here if logging is available
            ModelState.AddModelError("", "ユーザー登録中に予期せぬエラーが発生しました。時間を置いて再度お試しいただくか、管理者にお問い合わせください。");
        }
        
        return View(model);
    }

    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        return View(new LoginViewModel { ReturnUrl = returnUrl });
    }

    [HttpPost]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        try
        {
            var user = await _userManager.FindByEmailAsync(model.Email);

            if (user == null)
            {
                ModelState.AddModelError("", "メールアドレスまたはパスワードが正しくありません。入力内容をご確認のうえ、再度お試しください。");
                return View(model);
            }

            var result = await _signInManager.PasswordSignInAsync(
                user.UserName!,
                model.Password,
                model.RememberMe,
                lockoutOnFailure: true);

            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = "ログインしました。";
                return RedirectToAction("Index", "Home");
            }

            if (result.IsLockedOut)
            {
                ModelState.AddModelError("", "連続してログインに失敗したため、アカウントが一時的にロックされました。5分後に再度お試しください。");
                return View(model);
            }

            if (result.IsNotAllowed)
            {
                ModelState.AddModelError("", "このアカウントはまだ有効化されていません。メールを確認してください。");
                return View(model);
            }

            ModelState.AddModelError("", "メールアドレスまたはパスワードが正しくありません。入力内容をご確認のうえ、再度お試しください。");
        }
        catch (Exception)
        {
            // Log the exception here
            ModelState.AddModelError("", "ログイン処理中に予期せぬエラーが発生しました。時間を置いて再度お試しください。");
        }

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ExchangePoints()
    {
        // ログインユーザーを取得
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Challenge();

        // 10ポイント必要（テスト用）
        if (user.Points < 10)
        {
            TempData["ErrorMessage"] = "ポイントが不足しています。（10ポイント必要です）";
            return RedirectToAction("Index", "Home");
        }

        // ポイント消費
        user.Points -= 10;
        
        // 広告非表示期間を付与（24時間）
        // 現在の期限が切れていれば現在から24時間、そうでなければ既存の期限から24時間延長
        if (user.AdFreeUntil == null || user.AdFreeUntil < DateTime.UtcNow)
        {
            user.AdFreeUntil = DateTime.UtcNow.AddHours(24);
        }
        else
        {
            user.AdFreeUntil = user.AdFreeUntil.Value.AddHours(24);
        }

        await _userManager.UpdateAsync(user);
        
        TempData["SuccessMessage"] = "10ポイントを消費し、広告非表示特典を獲得しました！";
        return RedirectToAction("Index", "Home");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction("Index", "Home");
    }

    private void AddIdentityErrors(IdentityResult result)
    {
        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }
    }
}
