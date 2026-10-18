
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;

namespace RaceDay.Api.Controllers;

[ApiController]
[Route("api/profile")]
[Authorize]
public class ProfileController : ControllerBase
{
    private readonly RaceDayDbContext _context;

    public ProfileController(RaceDayDbContext context)
    {
        _context = context;
    }

    // GET: api/profile/me
    // Returns the profile of the currently logged-in user.
    [HttpGet("me")]
    public async Task<IActionResult> GetMyProfile()
    {
        // Read the user's ID from the validated JWT.
        string? userIdValue = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        // Reject the request if the ID is missing or invalid.
        if (!int.TryParse(userIdValue, out int userId))
        {
            return Unauthorized(new
            {
                message = "The token does not contain a valid user ID."
            });
        }

        // Find the logged-in user's record in the database.
        var user = await _context.Users
            .AsNoTracking()
            .Where(user => user.UserId == userId)
            .Select(user => new
            {
                user.UserId,
                user.FirstName,
                user.LastName,
                user.Email,
                user.Role,
                user.CreatedAt
            })
            .SingleOrDefaultAsync();

        // Handle a user ID that no longer exists.
        if (user == null)
        {
            return NotFound(new
            {
                message = "User profile not found."
            });
        }

        // Return only the safe profile information.
        return Ok(user);

    }

    // PUT: api/profile/me
    // Updates the profile of the currently logged-in user.
    [HttpPut("me")]
    public async Task<IActionResult> UpdateMyProfile(
        [FromBody] RaceDay.Api.Dtos.UpdateProfileRequest request)
    {
        // Get the logged-in user's ID from their validated JWT.
        string? userIdValue = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        if (!int.TryParse(userIdValue, out int userId))
        {
            return Unauthorized(new
            {
                message = "The token does not contain a valid user ID."
            });
        }

        // Find the user's existing database record.
        var user = await _context.Users
            .SingleOrDefaultAsync(user => user.UserId == userId);

        if (user == null)
        {
            return NotFound(new
            {
                message = "User profile not found."
            });
        }

        // Clean up the submitted values.
        string firstName = request.FirstName.Trim();
        string lastName = request.LastName.Trim();
        string email = request.Email.Trim().ToLowerInvariant();

        // Reject empty names.
        if (string.IsNullOrWhiteSpace(firstName) ||
            string.IsNullOrWhiteSpace(lastName))
        {
            return BadRequest(new
            {
                message = "First name and last name are required."
            });
        }

        // Reject an email already used by another account.
        bool emailExists = await _context.Users.AnyAsync(
            existingUser =>
                existingUser.Email == email &&
                existingUser.UserId != userId);

        if (emailExists)
        {
            return Conflict(new
            {
                message = "Another account already uses this email address."
            });
        }

        // Update only the fields permitted by this endpoint.
        user.FirstName = firstName;
        user.LastName = lastName;
        user.Email = email;

        await _context.SaveChangesAsync();

        // Return safe profile information, never the password hash.
        return Ok(new
        {
            message = "Profile updated successfully.",
            user = new
            {
                user.UserId,
                user.FirstName,
                user.LastName,
                user.Email,
                user.Role,
                user.CreatedAt
            }
        });
    }

    // GET: api/profile/me/enrolments
    // Returns enrolments belonging to the logged-in participant.
    [HttpGet("me/enrolments")]
    public async Task<IActionResult> GetMyEnrolments()
    {
        // Get the logged-in user's ID from the validated JWT.
        string? userIdValue = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        if (!int.TryParse(userIdValue, out int userId))
        {
            return Unauthorized(new
            {
                message = "The token does not contain a valid user ID."
            });
        }

        // Find only this participant's enrolments.
        var enrolments = await _context.Enrolments
            .AsNoTracking()
            .Where(enrolment => enrolment.UserId == userId)
            .Select(enrolment => new
            {
                enrolment.EnrolmentId,
                enrolment.UserId,
                enrolment.CategoryId,
                enrolment.EnrolmentDate,
                enrolment.Status,

                Category = enrolment.Category == null
                    ? null
                    : new
                    {
                        enrolment.Category.CategoryName,
                        enrolment.Category.DistanceKm,
                        enrolment.Category.EntryFee,

                        Event = enrolment.Category.Event == null
                            ? null
                            : new
                            {
                                enrolment.Category.Event.EventId,
                                enrolment.Category.Event.EventName,
                                enrolment.Category.Event.EventDate,
                                enrolment.Category.Event.Location,
                                enrolment.Category.Event.EventType
                            }
                    }
            })
            .ToListAsync();

        return Ok(enrolments);
    }


}
