using System;
using System.ComponentModel.DataAnnotations;

namespace RaceDay.Api.Models;

public class User
{
    [Key]
    public int UserId { get; set; }

    [Required]
    [MaxLength(50)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string LastName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [MaxLength(255)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MaxLength(255)]
    public string PasswordHash { get; set; } = string.Empty;

    [Required]
    [MaxLength(20)]
    public string Role { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public List<RaceEvent> OrganisedEvents { get; set; } = new();

    public List<Enrolment> Enrolments { get; set; } = new();
}