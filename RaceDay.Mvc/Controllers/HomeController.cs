using Microsoft.AspNetCore.Mvc;
using RaceDay.Mvc.Models;
using RaceDay.Mvc.Services;

namespace RaceDay.Mvc.Controllers;

public class HomeController(RaceDayApiClient api) : Controller
{
    public async Task<IActionResult> Index(string? search, string? eventType)
    {
        var events = (await api.GetAsync<List<EventViewModel>>("api/events") ?? new())
            .Where(e => e.EventDate.Date >= DateTime.Today)
            .ToList();
        if (!string.IsNullOrWhiteSpace(search))
            events = events.Where(e => e.EventName.Contains(search, StringComparison.OrdinalIgnoreCase) || e.Location.Contains(search, StringComparison.OrdinalIgnoreCase)).ToList();
        if (!string.IsNullOrWhiteSpace(eventType)) events = events.Where(e => e.EventType == eventType).ToList();
        ViewBag.Search = search;
        ViewBag.EventType = eventType;
        return View(events);
    }

    public IActionResult Error() => View();
}
