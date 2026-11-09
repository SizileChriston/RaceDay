
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.Dtos;
using RaceDay.Api.Models;

namespace RaceDay.Api.Controllers;

[ApiController]
[Route("api/enrolments")]
[Authorize]
public class EnrolmentsController : ControllerBase
{
    private readonly RaceDayDbContext _context;

    public EnrolmentsController(RaceDayDbContext context)
    {
        _context = context;
    }

    // POST: api/enrolments
    // Allows a participant to enrol in a category.
    [HttpPost]
    [Authorize(Roles = "Participant")]
    public async Task<IActionResult> CreateEnrolment(
        [FromBody] CreateEnrolmentRequest request)
    {
        // Get the participant ID from the authenticated JWT.
        string? userIdValue = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        if (!int.TryParse(userIdValue, out int userId))
        {
            return Unauthorized(new
            {
                message = "A valid user ID could not be found in the token."
            });
        }

        // Find the selected category and its event.
        var category = await _context.Categories
            .Include(c => c.Event)
            .FirstOrDefaultAsync(c => c.CategoryId == request.CategoryId);

        if (category == null)
        {
            return NotFound(new
            {
                message = "The selected category was not found."
            });
        }

        // Check whether this participant has enrolled before.
        var existingEnrolment = await _context.Enrolments
            .FirstOrDefaultAsync(e =>
                e.UserId == userId &&
                e.CategoryId == request.CategoryId);

        // Do not allow another active enrolment.
        if (existingEnrolment != null &&
            existingEnrolment.Status != "Cancelled")
        {
            return Conflict(new
            {
                message = "You are already enrolled in this category."
            });
        }

        // Count all enrolments that are not cancelled.
        int currentEnrolments = await _context.Enrolments
            .CountAsync(e =>
                e.CategoryId == request.CategoryId &&
                e.Status != "Cancelled");

        // Check whether the category has reached capacity.
        if (currentEnrolments >= category.MaxParticipants)
        {
            return Conflict(new
            {
                message = "This category has reached its maximum capacity."
            });
        }

        // Reactivate a previously cancelled enrolment.
        if (existingEnrolment != null)
        {
            existingEnrolment.Status = "Confirmed";
            existingEnrolment.EnrolmentDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Your previous enrolment has been reactivated.",
                existingEnrolment.EnrolmentId,
                existingEnrolment.UserId,
                existingEnrolment.CategoryId,
                existingEnrolment.EnrolmentDate,
                existingEnrolment.Status
            });
        }

        // Create a new enrolment.
        var enrolment = new Enrolment
        {
            UserId = userId,
            CategoryId = request.CategoryId,
            EnrolmentDate = DateTime.UtcNow,
            Status = "Confirmed"
        };

        _context.Enrolments.Add(enrolment);

        await _context.SaveChangesAsync();

        return Created(
            $"/api/profile/me/enrolments",
            new
            {
                enrolment.EnrolmentId,
                enrolment.UserId,
                enrolment.CategoryId,
                enrolment.EnrolmentDate,
                enrolment.Status
            });
    }

    // PUT: api/enrolments/{id}/cancel
    // Allows a participant to cancel their own enrolment.
    [HttpPut("{id:int}/cancel")]
    [Authorize(Roles = "Participant")]
    public async Task<IActionResult> CancelEnrolment(int id)
    {
        // Get the logged-in participant's ID.
        string? userIdValue = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        if (!int.TryParse(userIdValue, out int userId))
        {
            return Unauthorized(new
            {
                message = "A valid user ID could not be found in the token."
            });
        }

        // Find the enrolment belonging to this participant.
        var enrolment = await _context.Enrolments
            .Include(e => e.Result)
            .FirstOrDefaultAsync(e =>
                e.EnrolmentId == id &&
                e.UserId == userId);

        if (enrolment == null)
        {
            return NotFound(new
            {
                message = "Enrolment not found."
            });
        }

        // Do not cancel an enrolment that already has a result.
        if (enrolment.Result != null)
        {
            return Conflict(new
            {
                message = "An enrolment with a recorded result cannot be cancelled."
            });
        }

        if (enrolment.Status == "Cancelled")
        {
            return Conflict(new
            {
                message = "This enrolment has already been cancelled."
            });
        }

        enrolment.Status = "Cancelled";

        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = "Enrolment cancelled successfully.",
            enrolment.EnrolmentId,
            enrolment.Status
        });
    }

    // GET: api/events/{eventId}/enrolments
    // Allows an organiser to view enrolments for their own event.
    [HttpGet("/api/events/{eventId:int}/enrolments")]
    [Authorize(Roles = "Organiser")]
    public async Task<IActionResult> GetEventEnrolments(int eventId)
    {
        // Get the logged-in organiser's ID.
        string? userIdValue = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        if (!int.TryParse(userIdValue, out int organiserId))
        {
            return Unauthorized(new
            {
                message = "A valid user ID could not be found in the token."
            });
        }

        // Check whether the event exists.
        var raceEvent = await _context.Events
            .FirstOrDefaultAsync(e => e.EventId == eventId);

        if (raceEvent == null)
        {
            return NotFound(new
            {
                message = $"Event with ID {eventId} was not found."
            });
        }

        // Confirm that the organiser owns this event.
        if (raceEvent.OrganiserId != organiserId)
        {
            return Forbid();
        }

        // Retrieve enrolments belonging to categories in this event.
        var enrolments = await _context.Enrolments
            .AsNoTracking()
            .Where(e => e.Category != null &&
                        e.Category.EventId == eventId)
            .OrderBy(e => e.EnrolmentDate)
            .Select(e => new
            {
                e.EnrolmentId,
                e.UserId,
                Participant = e.User == null
                    ? null
                    : new
                    {
                        e.User.FirstName,
                        e.User.LastName,
                        e.User.Email
                    },
                e.CategoryId,
                CategoryName = e.Category!.CategoryName,
                e.EnrolmentDate,
                e.Status
            })
            .ToListAsync();

        return Ok(new
        {
            EventId = raceEvent.EventId,
            raceEvent.EventName,
            TotalEnrolments = enrolments.Count,
            Enrolments = enrolments
        });
    }
}
