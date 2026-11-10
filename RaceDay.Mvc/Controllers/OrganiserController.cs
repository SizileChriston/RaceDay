using System.Globalization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RaceDay.Mvc.Models;
using RaceDay.Mvc.Services;

namespace RaceDay.Mvc.Controllers;

[Authorize(Roles = "Organiser")]
public class OrganiserController(RaceDayApiClient api) : Controller
{
    public async Task<IActionResult> Dashboard()
    {
        var events = await api.GetAsync<List<EventViewModel>>("api/events/mine") ?? new();
        foreach (var item in events)
        {
            var summary = await api.GetAsync<System.Text.Json.JsonElement>($"api/events/{item.EventId}/enrolments");
            if (summary.ValueKind == System.Text.Json.JsonValueKind.Object && summary.TryGetProperty("totalEnrolments", out var total))
                item.TotalEnrolments = total.GetInt32();
        }
        return View(events);
    }

    [HttpGet] public IActionResult CreateEvent() => View(new EventViewModel());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateEvent(EventViewModel model, IFormFile? banner)
    {
        if (!ModelState.IsValid) return View(model);
        var created = await api.SendAsync<EventViewModel>(HttpMethod.Post, "api/events", model);
        if (!created.Success || created.Data is null) { ModelState.AddModelError(string.Empty, created.Error ?? "Could not create event."); return View(model); }
        if (banner is { Length: > 0 })
        {
            var upload = await api.UploadAsync($"api/events/{created.Data.EventId}/banner", banner);
            if (!upload.Success) TempData["Error"] = upload.Error ?? "Event created, but its banner upload failed.";
        }
        TempData["Message"] = "Event created.";
        return RedirectToAction(nameof(ManageEvent), new { id = created.Data.EventId });
    }

    public async Task<IActionResult> ManageEvent(int id)
    {
        var item = await api.GetAsync<EventViewModel>($"api/events/{id}");
        if (item is null) return NotFound();
        ViewBag.Categories = await api.GetAsync<List<CategoryViewModel>>($"api/events/{id}/categories") ?? new();
        ViewBag.Enrolments = new List<EnrolmentViewModel>();
        var data = await api.GetAsync<System.Text.Json.JsonElement>($"api/events/{id}/enrolments");
        if (data is System.Text.Json.JsonElement json && json.TryGetProperty("enrolments", out var rows))
            ViewBag.Enrolments = System.Text.Json.JsonSerializer.Deserialize<List<EnrolmentViewModel>>(rows.GetRawText(), new System.Text.Json.JsonSerializerOptions(System.Text.Json.JsonSerializerDefaults.Web)) ?? new();
        var resultData = await api.GetAsync<System.Text.Json.JsonElement>($"api/events/{id}/results");
        ViewBag.Results = resultData.ValueKind == System.Text.Json.JsonValueKind.Object && resultData.TryGetProperty("results", out var resultRows)
            ? System.Text.Json.JsonSerializer.Deserialize<List<ResultViewModel>>(resultRows.GetRawText(), new System.Text.Json.JsonSerializerOptions(System.Text.Json.JsonSerializerDefaults.Web)) ?? new()
            : new List<ResultViewModel>();
        return View(item);
    }

    [HttpGet]
    public async Task<IActionResult> EditEvent(int id)
    {
        var item = await api.GetAsync<EventViewModel>($"api/events/{id}");
        return item is null ? NotFound() : View(item);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> EditEvent(EventViewModel model)
    {
        if (!ModelState.IsValid) return View(model);
        var result = await api.SendAsync<System.Text.Json.JsonElement>(HttpMethod.Put, $"api/events/{model.EventId}", model);
        TempData[result.Success ? "Message" : "Error"] = result.Success ? "Event details updated." : result.Error ?? "Could not update event.";
        return RedirectToAction(nameof(ManageEvent), new { id = model.EventId });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteEvent(int id)
    {
        var result = await api.SendAsync<System.Text.Json.JsonElement>(HttpMethod.Delete, $"api/events/{id}");
        TempData[result.Success ? "Message" : "Error"] = result.Success ? "Event deleted." : result.Error ?? "Could not delete event.";
        return RedirectToAction(nameof(Dashboard));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> AddCategory(int eventId, CategoryViewModel model)
    {
        var result = await api.SendAsync<System.Text.Json.JsonElement>(HttpMethod.Post, $"api/events/{eventId}/categories", new { model.CategoryName, model.DistanceKm, model.MaxParticipants, model.EntryFee });
        TempData[result.Success ? "Message" : "Error"] = result.Success ? "Category added." : result.Error ?? "Could not add category.";
        return RedirectToAction(nameof(ManageEvent), new { id = eventId });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> RecordResult(int eventId, ResultEntryViewModel model)
    {
        if (!ModelState.IsValid) { TempData["Error"] = "Check the result fields and try again."; return RedirectToAction(nameof(ManageEvent), new { id = eventId }); }
        if (!TimeSpan.TryParse(model.FinishTime, CultureInfo.InvariantCulture, out var time)) { TempData["Error"] = "Enter finish time as hours:minutes:seconds."; return RedirectToAction(nameof(ManageEvent), new { id = eventId }); }
        var result = await api.SendAsync<System.Text.Json.JsonElement>(HttpMethod.Post, $"api/events/{eventId}/results", new { model.EnrolmentId, finishTime = time, model.Position, model.ResultStatus });
        TempData[result.Success ? "Message" : "Error"] = result.Success ? "Result recorded." : result.Error ?? "Could not save result.";
        return RedirectToAction(nameof(ManageEvent), new { id = eventId });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> UploadBanner(int id, IFormFile file)
    {
        var result = await api.UploadAsync($"api/events/{id}/banner", file);
        TempData[result.Success ? "Message" : "Error"] = result.Success ? "Event banner uploaded." : result.Error ?? "Banner upload failed.";
        return RedirectToAction(nameof(ManageEvent), new { id });
    }
}
