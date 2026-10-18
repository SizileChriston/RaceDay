
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using RaceDay.Api.Dtos;

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

        // Mark the event for deletion.
        _context.Events.Remove(existingEvent);

        await _context.SaveChangesAsync();

        // HTTP 204 means deletion succeeded with no response body.
        return NoContent();
    }




}
