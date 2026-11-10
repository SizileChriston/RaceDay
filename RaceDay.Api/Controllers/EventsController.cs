
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using RaceDay.Api.Dtos;
using RaceDay.Api.Services;

namespace RaceDay.Api.Controllers;

[ApiController]
[Route("api/events")]
public class EventsController : ControllerBase
{
    private readonly RaceDayDbContext _context;

    public EventsController(RaceDayDbContext context)
    {
        _context = context;
    }

    // GET: /api/events
    // Public endpoint: anyone can view the available events.
    [HttpGet]
    public async Task<IActionResult> GetEvents()
    {
        var events = await _context.Events
            .AsNoTracking()
            .OrderBy(raceEvent => raceEvent.EventDate)
            .Select(raceEvent => new
            {
                raceEvent.EventId,
                raceEvent.EventName,
                raceEvent.Description,
                raceEvent.EventDate,
                raceEvent.Location,
                raceEvent.EventType,
                raceEvent.BannerImageUrl,
                raceEvent.CreatedAt
            })
            .ToListAsync();

        return Ok(events);
    }


    // GET: /api/events/{eventId}
    // Public endpoint: retrieves one event using its ID.
    [HttpGet("{eventId:int}")]
    public async Task<IActionResult> GetEventById(int eventId)
    {
        var raceEvent = await _context.Events
            .AsNoTracking()
            .Where(eventItem => eventItem.EventId == eventId)
            .Select(eventItem => new
            {
                eventItem.EventId,
                eventItem.EventName,
                eventItem.Description,
                eventItem.EventDate,
                eventItem.Location,
                eventItem.EventType,
                eventItem.BannerImageUrl,
                eventItem.CreatedAt
            })
            .FirstOrDefaultAsync();

        if (raceEvent == null)
        {
            return NotFound(new
            {
                message = $"Event with ID {eventId} was not found."
            });
        }

        return Ok(raceEvent);
    }

    // GET: api/events/mine
    // Organisers can see the events they own for their dashboard.
    [HttpGet("mine")]
    [Authorize(Roles = "Organiser")]
    public async Task<IActionResult> GetMyEvents()
    {
        string? userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(userIdValue, out int organiserId)) return Unauthorized();

        var events = await _context.Events.AsNoTracking()
            .Where(e => e.OrganiserId == organiserId)
            .OrderBy(e => e.EventDate)
            .Select(e => new { e.EventId, e.EventName, e.Description, e.EventDate, e.Location, e.EventType, e.BannerImageUrl, e.CreatedAt })
            .ToListAsync();
        return Ok(events);
    }

    // POST: api/events/{eventId}/banner
    // Upload an event image through the API and save its URL.
    [HttpPost("{eventId:int}/banner")]
    [Authorize(Roles = "Organiser")]
    [RequestSizeLimit(5_000_000)]
    public async Task<IActionResult> UploadBanner(int eventId, IFormFile file, [FromServices] IBlobStorageService blobStorage, CancellationToken cancellationToken)
    {
        string? userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(userIdValue, out int organiserId)) return Unauthorized();
        var raceEvent = await _context.Events.FirstOrDefaultAsync(e => e.EventId == eventId, cancellationToken);
        if (raceEvent is null) return NotFound(new { message = "Event not found." });
        if (raceEvent.OrganiserId != organiserId) return Forbid();
        if (file is null || file.Length == 0) return BadRequest(new { message = "Choose an image to upload." });
        if (!IsAllowedImage(file)) return BadRequest(new { message = "Only PNG, JPEG, and WebP images are accepted." });

        try
        {
            await using var stream = file.OpenReadStream();
            raceEvent.BannerImageUrl = await blobStorage.UploadImageAsync(stream, file.FileName, file.ContentType, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            return Ok(new { raceEvent.EventId, raceEvent.BannerImageUrl });
        }
        catch (InvalidOperationException ex) { return StatusCode(503, new { message = ex.Message }); }
        catch (Azure.RequestFailedException) { return StatusCode(503, new { message = "Azure Blob Storage could not complete the upload. Check the storage account settings and container access." }); }
    }

    private static bool IsAllowedImage(IFormFile file) => file.ContentType is "image/png" or "image/jpeg" or "image/webp" && file.Length <= 5_000_000;


    // POST: /api/events
    // Only authenticated Organisers can create events.
    [HttpPost]
    [Authorize(Roles = "Organiser")]
    public async Task<IActionResult> CreateEvent(
        [FromBody] CreateEventRequest request)
    {
        // Get the logged-in organiser's ID from the JWT token.
        var userIdValue = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        if (!int.TryParse(userIdValue, out var organiserId))
        {
            return Unauthorized(new
            {
                message = "A valid user ID could not be found in the token."
            });
        }

        // Accept only the supported event types.
        var allowedEventTypes = new[]
        {
            "Running",
            "Walking",
            "Cycling"
        };

        var eventType = request.EventType.Trim();

        if (!allowedEventTypes.Contains(
            eventType,
            StringComparer.OrdinalIgnoreCase))
        {
            return BadRequest(new
            {
                message = "Event type must be Running, Walking, or Cycling."
            });
        }

        // Create the event using the logged-in organiser's ID.
        var newEvent = new RaceDay.Api.Models.RaceEvent
        {
            OrganiserId = organiserId,
            EventName = request.EventName.Trim(),
            Description = request.Description?.Trim(),
            EventDate = request.EventDate!.Value,
            Location = request.Location.Trim(),
            EventType = allowedEventTypes.First(
                type => type.Equals(
                    eventType,
                    StringComparison.OrdinalIgnoreCase))
        };

        _context.Events.Add(newEvent);

        await _context.SaveChangesAsync();

        // Return HTTP 201 Created with the new event details.
        return CreatedAtAction(
            nameof(GetEventById),
            new { eventId = newEvent.EventId },
            new
            {
                newEvent.EventId,
                newEvent.EventName,
                newEvent.Description,
                newEvent.EventDate,
                newEvent.Location,
                newEvent.EventType,
                newEvent.CreatedAt
            });

    }


    // PUT: /api/events/{eventId}
    // Only the Organiser who owns the event can update it.
    [HttpPut("{eventId:int}")]
    [Authorize(Roles = "Organiser")]
    public async Task<IActionResult> UpdateEvent(
        int eventId,
        [FromBody] CreateEventRequest request)
    {
        // Identify the logged-in organiser using their JWT token.
        var userIdValue = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        if (!int.TryParse(userIdValue, out var organiserId))
        {
            return Unauthorized(new
            {
                message = "A valid user ID could not be found in the token."
            });
        }

        // Find the event in the database.
        var existingEvent = await _context.Events
            .FirstOrDefaultAsync(e => e.EventId == eventId);

        if (existingEvent == null)
        {
            return NotFound(new
            {
                message = $"Event with ID {eventId} was not found."
            });
        }

        // Prevent organisers from editing events they do not own.
        if (existingEvent.OrganiserId != organiserId)
        {
            return Forbid();
        }

        // Accept only supported event types.
        var allowedEventTypes = new[]
        {
            "Running",
            "Walking",
            "Cycling"
        };

        var eventType = request.EventType.Trim();

        if (!allowedEventTypes.Contains(
            eventType,
            StringComparer.OrdinalIgnoreCase))
        {
            return BadRequest(new
            {
                message = "Event type must be Running, Walking, or Cycling."
            });
        }

        // Update the existing event's details.
        existingEvent.EventName = request.EventName.Trim();
        existingEvent.Description = request.Description?.Trim();
        existingEvent.EventDate = request.EventDate!.Value;
        existingEvent.Location = request.Location.Trim();
        existingEvent.EventType = allowedEventTypes.First(
            type => type.Equals(
                eventType,
                StringComparison.OrdinalIgnoreCase));

        await _context.SaveChangesAsync();

        return Ok(new
        {
            existingEvent.EventId,
            existingEvent.EventName,
            existingEvent.Description,
            existingEvent.EventDate,
            existingEvent.Location,
            existingEvent.EventType,
            existingEvent.CreatedAt
        });
    }


    // DELETE: /api/events/{eventId}
    // Only the Organiser who owns the event can delete it.
    [HttpDelete("{eventId:int}")]
    [Authorize(Roles = "Organiser")]
    public async Task<IActionResult> DeleteEvent(int eventId)
    {
        // Get the logged-in Organiser's ID from the JWT token.
        var userIdValue = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        if (!int.TryParse(userIdValue, out var organiserId))
        {
            return Unauthorized(new
            {
                message = "A valid user ID could not be found in the token."
            });
        }

        // Find the event in the database.
        var existingEvent = await _context.Events
            .FirstOrDefaultAsync(e => e.EventId == eventId);

        if (existingEvent == null)
        {
            return NotFound(new
            {
                message = $"Event with ID {eventId} was not found."
            });
        }

        // Prevent an Organiser from deleting another Organiser's event.
        if (existingEvent.OrganiserId != organiserId)
        {
            return Forbid();
        }

        bool hasEnrolments = await _context.Enrolments.AnyAsync(e => e.Category != null && e.Category.EventId == eventId);
        if (hasEnrolments)
        {
            return Conflict(new { message = "This event has participant enrolments and cannot be deleted while those records exist." });
        }

        // Mark the event for deletion.
        _context.Events.Remove(existingEvent);

        await _context.SaveChangesAsync();

        // HTTP 204 means deletion succeeded with no response body.
        return NoContent();
    }




}
