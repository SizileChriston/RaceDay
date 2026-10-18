
using System.ComponentModel.DataAnnotations;

namespace RaceDay.Api.Dtos;

public class CreateCategoryRequest
{
    [Required]
    [StringLength(100)]
    public string CategoryName { get; set; } = string.Empty;

    [Range(0.01, 9999.99)]
    public decimal DistanceKm { get; set; }

    [Range(1, 100000)]
    public int MaxParticipants { get; set; }

    [Range(0.00, 999999.99)]
    public decimal EntryFee { get; set; }
}
