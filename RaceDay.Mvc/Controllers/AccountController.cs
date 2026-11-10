using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using RaceDay.Mvc.Models;
using RaceDay.Mvc.Services;

namespace RaceDay.Mvc.Controllers;

public class AccountController(RaceDayApiClient api) : Controller
{
    [HttpGet] public IActionResult Login(string? returnUrl = null) { ViewBag.ReturnUrl = returnUrl; return View(); }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(AccountViewModel model, string? returnUrl = null)
    {
        if (!ModelState.IsValid) return View(model);
        var result = await api.SendAsync<JsonElement>(HttpMethod.Post, "api/auth/login", new { model.Email, model.Password });
        var data = result.Data;
        if (!result.Success || data.ValueKind != JsonValueKind.Object || !data.TryGetProperty("accessToken", out var tokenValue))
        {
            ModelState.AddModelError(string.Empty, result.Error ?? "Login failed. Check that the API is running and your details are correct.");
            return View(model);
        }
        var user = data.GetProperty("user");
        var token = tokenValue.GetString()!;
        var userId = user.GetProperty("userId").ToString();
        var role = user.GetProperty("role").GetString()!;
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId),
            new(ClaimTypes.Name, user.GetProperty("firstName").GetString() ?? "RaceDay user"),
            new(ClaimTypes.Email, user.GetProperty("email").GetString() ?? model.Email),
            new(ClaimTypes.Role, role)
        };
        HttpContext.Session.SetString("AccessToken", token);
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme)));
        if (Url.IsLocalUrl(returnUrl)) return Redirect(returnUrl!);
        return RedirectToAction("Index", "Home");
    }

    [HttpGet] public IActionResult Register() => View();

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(AccountViewModel model)
    {
        if (!ModelState.IsValid) return View(model);
        if (string.IsNullOrWhiteSpace(model.FirstName) || string.IsNullOrWhiteSpace(model.LastName))
        {
            ModelState.AddModelError(string.Empty, "Enter your first and last name.");
            return View(model);
        }
        var result = await api.SendAsync<JsonElement>(HttpMethod.Post, "api/auth/register", new { model.FirstName, model.LastName, model.Email, model.Password });
        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.Error ?? "Registration failed.");
            return View(model);
        }
        TempData["Message"] = "Account created. Please sign in.";
        return RedirectToAction(nameof(Login));
    }

    [Authorize, HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        HttpContext.Session.Clear();
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Index", "Home");
    }

    public IActionResult Denied() => View();
}
