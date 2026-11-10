using System;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.Dtos;
using RaceDay.Api.Models;

namespace RaceDay.Api.Controllers;

[ApiController]
[Route("api")]
[Authorize]
public class ResultsController : ControllerBase
{
    private readonly RaceDayDbContext _context;

    public ResultsController(RaceDayDbContext context)
    {
        _context = context;
    }

    // GET: api/events/{eventId}/results
    // Organiser views results for their own event.
    [HttpGet("events/{eventId:int}/results")]
    [Authorize(Roles = "Organiser")]
    public async Task<IActionResult> GetEventResults(int eventId)
    {
        string? userIdValue = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        if (!int.TryParse(userIdValue, out int organiserId))
        {
            return Unauthorized(new
            {
                message = "A valid user ID could not be found in the token."
            });
        }

        var raceEvent = await _context.Events
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.EventId == eventId);

        if (raceEvent == null)
        {
            return NotFound(new
            {
                message = $"Event with ID {eventId} was not found."
            });
        }

        if (raceEvent.OrganiserId != organiserId)
        {
            return Forbid();
        }

        var results = await _context.Results
            .AsNoTracking()
            .Where(r =>
                r.Enrolment != null &&
                r.Enrolment.Category != null &&
                r.Enrolment.Category.EventId == eventId)
            .OrderBy(r => r.Position)
            .Select(r => new
            {
                r.ResultId,
                r.EnrolmentId,
                r.FinishTime,
                r.Position,
                r.ResultStatus,
                r.RecordedAt,

                Participant = r.Enrolment!.User == null
                    ? null
                    : new
                    {
                        r.Enrolment.User.FirstName,
                        r.Enrolment.User.LastName,
                        r.Enrolment.User.Email
                    },

                Category = r.Enrolment!.Category == null
                    ? null
                    : new
                    {
                        r.Enrolment.Category.CategoryId,
                        r.Enrolment.Category.CategoryName,
                        r.Enrolment.Category.DistanceKm
                    }
            })
            .ToListAsync();

        return Ok(new
        {
            EventId = raceEvent.EventId,
            raceEvent.EventName,
            TotalResults = results.Count,
            Results = results
        });
    }

    // POST: api/events/{eventId}/results
    // Organiser records a result.
    [HttpPost("events/{eventId:int}/results")]
    [Authorize(Roles = "Organiser")]
    public async Task<IActionResult> CreateResult(
        int eventId,
        [FromBody] CreateResultRequest request)
    {
        string? userIdValue = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        if (!int.TryParse(userIdValue, out int organiserId))
        {
            return Unauthorized(new
            {
                message = "A valid user ID could not be found in the token."
            });
        }

        var raceEvent = await _context.Events
            .FirstOrDefaultAsync(e => e.EventId == eventId);

        if (raceEvent == null)
        {
            return NotFound(new
            {
                message = $"Event with ID {eventId} was not found."
            });
        }

        if (raceEvent.OrganiserId != organiserId)
        {
            return Forbid();
        }

        var allowedStatuses = new[]
        {
            "Official",
            "Pending",
            "Disqualified"
        };

        if (string.IsNullOrWhiteSpace(request.ResultStatus))
        {
            return BadRequest(new
            {
                message = "A result status is required."
            });
        }

        string resultStatus = request.ResultStatus.Trim();

        if (!allowedStatuses.Contains(
            resultStatus,
            StringComparer.OrdinalIgnoreCase))
        {
            return BadRequest(new
            {
                message = "Result status must be Official, Pending, or Disqualified."
            });
        }

        if (request.FinishTime.HasValue &&
            request.FinishTime.Value < TimeSpan.Zero)
        {
            return BadRequest(new
            {
                message = "Finish time cannot be negative."
            });
        }

        var enrolment = await _context.Enrolments
            .Include(e => e.Category)
            .FirstOrDefaultAsync(e =>
                e.EnrolmentId == request.EnrolmentId);

        if (enrolment == null ||
            enrolment.Category == null ||
            enrolment.Category.EventId != eventId)
        {
            return NotFound(new
            {
                message = "The enrolment was not found for this event."
            });
        }

        if (enrolment.Status == "Cancelled")
        {
            return Conflict(new
            {
                message = "A cancelled enrolment cannot receive a result."
            });
        }

        bool resultExists = await _context.Results
            .AnyAsync(r => r.EnrolmentId == request.EnrolmentId);

        if (resultExists)
        {
            return Conflict(new
            {
                message = "A result already exists for this enrolment."
            });
        }

        var result = new Result
        {
            EnrolmentId = enrolment.EnrolmentId,
            FinishTime = request.FinishTime,
            Position = request.Position,
            ResultStatus = allowedStatuses.First(
                status => status.Equals(
                    resultStatus,
                    StringComparison.OrdinalIgnoreCase)),
            RecordedAt = DateTime.UtcNow
        };

        _context.Results.Add(result);

        await _context.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetEventResults),
            new { eventId },
            new
            {
                result.ResultId,
                result.EnrolmentId,
                result.FinishTime,
                result.Position,
                result.ResultStatus,
                result.RecordedAt
            });
    }

    // PUT: api/results/{id}
    // Organiser updates a result for their own event.
    [HttpPut("results/{id:int}")]
    [Authorize(Roles = "Organiser")]
    public async Task<IActionResult> UpdateResult(
        int id,
        [FromBody] UpdateResultRequest request)
    {
        string? userIdValue = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        if (!int.TryParse(userIdValue, out int organiserId))
        {
            return Unauthorized(new
            {
                message = "A valid user ID could not be found in the token."
            });
        }

        var result = await _context.Results
            .Include(r => r.Enrolment)
                .ThenInclude(e => e!.Category)
            .FirstOrDefaultAsync(r => r.ResultId == id);

        if (result == null ||
            result.Enrolment == null ||
            result.Enrolment.Category == null)
        {
            return NotFound(new
            {
                message = $"Result with ID {id} was not found."
            });
        }

        var raceEvent = await _context.Events
            .FirstOrDefaultAsync(e =>
                e.EventId == result.Enrolment.Category.EventId);

        if (raceEvent == null)
        {
            return NotFound(new
            {
                message = "The event connected to this result was not found."
            });
        }

        if (raceEvent.OrganiserId != organiserId)
        {
            return Forbid();
        }

        var allowedStatuses = new[]
        {
            "Official",
            "Pending",
            "Disqualified"
        };

        if (string.IsNullOrWhiteSpace(request.ResultStatus))
        {
            return BadRequest(new
            {
                message = "A result status is required."
            });
        }

        string resultStatus = request.ResultStatus.Trim();

        if (!allowedStatuses.Contains(
            resultStatus,
            StringComparer.OrdinalIgnoreCase))
        {
            return BadRequest(new
            {
                message = "Result status must be Official, Pending, or Disqualified."
            });
        }

        if (request.FinishTime.HasValue &&
            request.FinishTime.Value < TimeSpan.Zero)
        {
            return BadRequest(new
            {
                message = "Finish time cannot be negative."
            });
        }

        result.FinishTime = request.FinishTime;
        result.Position = request.Position;
        result.ResultStatus = allowedStatuses.First(
            status => status.Equals(
                resultStatus,
                StringComparison.OrdinalIgnoreCase));

        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = "Result updated successfully.",
            result.ResultId,
            result.EnrolmentId,
            result.FinishTime,
            result.Position,
            result.ResultStatus,
            result.RecordedAt
        });
    }

    // GET: api/profile/me/results
    // Participant views only their own results.
    [HttpGet("profile/me/results")]
    [Authorize(Roles = "Participant")]
    public async Task<IActionResult> GetMyResults()
    {
        string? userIdValue = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        if (!int.TryParse(userIdValue, out int userId))
        {
            return Unauthorized(new
            {
                message = "A valid user ID could not be found in the token."
            });
        }

        var results = await _context.Results
            .AsNoTracking()
            .Where(r =>
                r.Enrolment != null &&
                r.Enrolment.UserId == userId)
            .OrderBy(r => r.Position)
            .Select(r => new
            {
                r.ResultId,
                r.EnrolmentId,
                r.FinishTime,
                r.Position,
                r.ResultStatus,
                r.RecordedAt,

                Category = r.Enrolment!.Category == null
                    ? null
                    : new
                    {
                        r.Enrolment.Category.CategoryName,
                        r.Enrolment.Category.DistanceKm,

                        Event = r.Enrolment.Category.Event == null
                            ? null
                            : new
                            {
                                r.Enrolment.Category.Event.EventId,
                                r.Enrolment.Category.Event.EventName,
                                r.Enrolment.Category.Event.EventDate,
                                r.Enrolment.Category.Event.Location,
                                r.Enrolment.Category.Event.EventType
                            }
                    }
            })
            .ToListAsync();

        return Ok(results);
    }
}