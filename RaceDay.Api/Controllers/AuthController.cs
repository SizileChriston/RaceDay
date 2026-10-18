using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.Dtos;
using RaceDay.Api.Models;
using RaceDay.Api.Services;

namespace RaceDay.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly RaceDayDbContext _context;
    private readonly TokenService _tokenService;
    private readonly PasswordHasher<User> _passwordHasher;

    public AuthController(
        RaceDayDbContext context,
        TokenService tokenService)
    {
        _context = context;
        _tokenService = tokenService;
        _passwordHasher = new PasswordHasher<User>();
    }

    [HttpPost("register")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Register(
        RegisterRequest request)
    {
        string email = request.Email.Trim().ToLowerInvariant();

        bool emailExists = await _context.Users
            .AnyAsync(user => user.Email == email);

        if (emailExists)
        {
            return Conflict(new
            {
                message = "An account already uses this email address."
            });
        }

        User user = new User
        {
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            Email = email,
            Role = "Participant"
        };

        user.PasswordHash = _passwordHasher.HashPassword(
            user,
            request.Password);

        _context.Users.Add(user);

        await _context.SaveChangesAsync();

        return StatusCode(
            StatusCodes.Status201Created,
            new
            {
                user.UserId,
                user.FirstName,
                user.LastName,
                user.Email,
                user.Role
            });
    }

    [HttpPost("login")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login(
        LoginRequest request)
    {
        string email = request.Email.Trim().ToLowerInvariant();

        User? user = await _context.Users
            .SingleOrDefaultAsync(user => user.Email == email);

        if (user == null)
        {
            return Unauthorized(new
            {
                message = "Email address or password is incorrect."
            });
        }

        PasswordVerificationResult passwordResult =
            _passwordHasher.VerifyHashedPassword(
                user,
                user.PasswordHash,
                request.Password);

        if (passwordResult == PasswordVerificationResult.Failed)
        {
            return Unauthorized(new
            {
                message = "Email address or password is incorrect."
            });
        }

        string token = _tokenService.CreateToken(user);

        return Ok(new
        {
            accessToken = token,
            user = new
            {
                user.UserId,
                user.FirstName,
                user.LastName,
                user.Email,
                user.Role
            }
        });
    }


    // Temporary endpoint to test JWT authentication.
    [Microsoft.AspNetCore.Authorization.Authorize]
    [HttpGet("protected-test")]
    public IActionResult ProtectedTest()
    {
        return Ok(new
        {
            message = "JWT authentication successful!",
            user = User.Identity?.Name ?? "Authenticated user"
        });
    }


}