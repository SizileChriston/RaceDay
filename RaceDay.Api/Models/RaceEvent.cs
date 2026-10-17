using System;
using System.ComponentModel.DataAnnotations;

namespace RaceDay.Api.Models;

public class RaceEvent
{
    [Key]
    public int EventId { get; set; }

    public int OrganiserId { get; set; }

    [Required]
    [MaxLength(100)]
    public string EventName { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    public DateTime EventDate { get; set; }

    [Required]
    [MaxLength(150)]
    public string Location { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string EventType { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public User? Organiser { get; set; }

    public List<Category> Categories { get; set; } = new();

    public List<RaceRoute> Routes { get; set; } = new();
}