using System.ComponentModel.DataAnnotations;

namespace RaceDay.Api.Models;

public class RaceRoute
{
    [Key]
    public int RouteId { get; set; }

    public int EventId { get; set; }

    [Required]
    [MaxLength(100)]
    public string RouteName { get; set; } = string.Empty;

    [Range(typeof(decimal), "0.01", "9999.99")]
    public decimal DistanceKm { get; set; }

    [MaxLength(500)]
    public string? Description { get; set; }

    public RaceEvent? Event { get; set; }
}