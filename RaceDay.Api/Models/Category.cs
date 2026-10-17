using System.ComponentModel.DataAnnotations;

namespace RaceDay.Api.Models;

public class Category
{
    public int CategoryId { get; set; }

    public int EventId { get; set; }

    [Required]
    [MaxLength(100)]
    public string CategoryName { get; set; } = string.Empty;

    [Range(typeof(decimal), "0.01", "9999.99")]
    public decimal DistanceKm { get; set; }

    [Range(1, 100000)]
    public int MaxParticipants { get; set; }

    [Range(typeof(decimal), "0.00", "999999.99")]
    public decimal EntryFee { get; set; }

    public RaceEvent? Event { get; set; }

    public List<Enrolment> Enrolments { get; set; } = new();
}