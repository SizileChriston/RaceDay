
using System.ComponentModel.DataAnnotations;

namespace RaceDay.Api.Dtos;

public class CreateEnrolmentRequest
{
    // The participant must select a valid category ID.
    [Range(1, int.MaxValue)]
    public int CategoryId { get; set; }
}
