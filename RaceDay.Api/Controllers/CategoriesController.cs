
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.Dtos;
using RaceDay.Api.Models;
using System.Security.Claims;

namespace RaceDay.Api.Controllers;

[ApiController]
[Route("api")]
public class CategoriesController : ControllerBase
{
    private readonly RaceDayDbContext _context;

    public CategoriesController(RaceDayDbContext context)
    {
        _context = context;
    }

    // GET: /api/events/{eventId}/categories
    // Public: view categories belonging to an event.
    [HttpGet("events/{eventId:int}/categories")]
    public async Task<IActionResult> GetEventCategories(int eventId)
    {
        var eventExists = await _context.Events
            .AnyAsync(e => e.EventId == eventId);

        if (!eventExists)
        {
            return NotFound(new
            {
                message = $"Event with ID {eventId} was not found."
            });
        }

        var categories = await _context.Categories
            .AsNoTracking()
            .Where(c => c.EventId == eventId)
            .OrderBy(c => c.DistanceKm)
            .Select(c => new
            {
                c.CategoryId,
                c.EventId,
                c.CategoryName,
                c.DistanceKm,
                c.MaxParticipants,
                c.EntryFee
            })
            .ToListAsync();

        return Ok(categories);
    }

    // POST: /api/events/{eventId}/categories
    // Only the organiser who owns the event can add categories.
    [HttpPost("events/{eventId:int}/categories")]
    [Authorize(Roles = "Organiser")]
    public async Task<IActionResult> CreateCategory(
        int eventId,
        [FromBody] CreateCategoryRequest request)
    {
        var userIdValue = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        if (!int.TryParse(userIdValue, out var organiserId))
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

        var category = new Category
        {
            EventId = eventId,
            CategoryName = request.CategoryName.Trim(),
            DistanceKm = request.DistanceKm,
            MaxParticipants = request.MaxParticipants,
            EntryFee = request.EntryFee
        };

        _context.Categories.Add(category);
        await _context.SaveChangesAsync();

        return Created(
            $"/api/events/{eventId}/categories",
            new
            {
                category.CategoryId,
                category.EventId,
                category.CategoryName,
                category.DistanceKm,
                category.MaxParticipants,
                category.EntryFee
            });
    }

    // PUT: /api/categories/{categoryId}
    // Only the organiser who owns the category's event can update it.
    [HttpPut("categories/{categoryId:int}")]
    [Authorize(Roles = "Organiser")]
    public async Task<IActionResult> UpdateCategory(
        int categoryId,
        [FromBody] CreateCategoryRequest request)
    {
        var userIdValue = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        if (!int.TryParse(userIdValue, out var organiserId))
        {
            return Unauthorized(new
            {
                message = "A valid user ID could not be found in the token."
            });
        }

        var category = await _context.Categories
            .Include(c => c.Event)
            .FirstOrDefaultAsync(c => c.CategoryId == categoryId);

        if (category == null)
        {
            return NotFound(new
            {
                message = $"Category with ID {categoryId} was not found."
            });
        }

        if (category.Event == null ||
            category.Event.OrganiserId != organiserId)
        {
            return Forbid();
        }

        category.CategoryName = request.CategoryName.Trim();
        category.DistanceKm = request.DistanceKm;
        category.MaxParticipants = request.MaxParticipants;
        category.EntryFee = request.EntryFee;

        await _context.SaveChangesAsync();

        return Ok(new
        {
            category.CategoryId,
            category.EventId,
            category.CategoryName,
            category.DistanceKm,
            category.MaxParticipants,
            category.EntryFee
        });
    }

    // DELETE: /api/categories/{categoryId}
    // Only the organiser who owns the category's event can delete it.
    [HttpDelete("categories/{categoryId:int}")]
    [Authorize(Roles = "Organiser")]
    public async Task<IActionResult> DeleteCategory(int categoryId)
    {
        var userIdValue = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        if (!int.TryParse(userIdValue, out var organiserId))
        {
            return Unauthorized(new
            {
                message = "A valid user ID could not be found in the token."
            });
        }

        var category = await _context.Categories
            .Include(c => c.Event)
            .FirstOrDefaultAsync(c => c.CategoryId == categoryId);

        if (category == null)
        {
            return NotFound(new
            {
                message = $"Category with ID {categoryId} was not found."
            });
        }

        if (category.Event == null ||
            category.Event.OrganiserId != organiserId)
        {
            return Forbid();
        }

        _context.Categories.Remove(category);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
