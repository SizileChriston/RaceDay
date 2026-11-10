using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RaceDay.Mvc.Models;
using RaceDay.Mvc.Services;

namespace RaceDay.Mvc.Controllers;

public class ParticipantController(RaceDayApiClient api) : Controller
{
    public async Task<IActionResult> Details(int id)
    {
        var item = await api.GetAsync<EventViewModel>($"api/events/{id}");
        if (item is null) return NotFound();
        ViewBag.Categories = await api.GetAsync<List<CategoryViewModel>>($"api/events/{id}/categories") ?? new();
        return View(item);
    }

    [Authorize(Roles = "Participant"), HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Enter(int categoryId)
    {
        var result = await api.SendAsync<System.Text.Json.JsonElement>(HttpMethod.Post, "api/enrolments", new { categoryId });
        TempData[result.Success ? "Message" : "Error"] = result.Success ? "Your event entry is confirmed." : result.Error ?? "We could not complete your entry.";
        return RedirectToAction(nameof(MyEvents));
    }

    [Authorize(Roles = "Participant")]
    public async Task<IActionResult> MyEvents() => View(await api.GetAsync<List<EnrolmentViewModel>>("api/profile/me/enrolments") ?? new());
    [Authorize(Roles = "Participant")]
    public async Task<IActionResult> MyResults() => View(await api.GetAsync<List<ResultViewModel>>("api/profile/me/results") ?? new());

    [Authorize(Roles = "Participant")]
    public async Task<IActionResult> Profile()
    {
        var profile = await api.GetAsync<ProfileViewModel>("api/profile/me");
        return profile is null ? NotFound() : View(profile);
    }

    [Authorize(Roles = "Participant"), HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Profile(ProfileViewModel model)
    {
        if (!ModelState.IsValid) return View(model);
        var result = await api.SendAsync<System.Text.Json.JsonElement>(HttpMethod.Put, "api/profile/me", new { model.FirstName, model.LastName, model.Email });
        TempData[result.Success ? "Message" : "Error"] = result.Success ? "Your profile was updated." : result.Error ?? "Profile update failed.";
        return RedirectToAction(nameof(Profile));
    }

    [Authorize(Roles = "Participant"), HttpPost, ValidateAntiForgeryToken, RequestSizeLimit(5_000_000)]
    public async Task<IActionResult> UploadProfilePicture(IFormFile file)
    {
        if (file is null || file.Length == 0) { TempData["Error"] = "Choose an image first."; return RedirectToAction(nameof(Profile)); }
        var result = await api.UploadAsync("api/profile/me/picture", file);
        TempData[result.Success ? "Message" : "Error"] = result.Success ? "Profile picture uploaded." : result.Error ?? "Image upload failed.";
        return RedirectToAction(nameof(Profile));
    }
}
