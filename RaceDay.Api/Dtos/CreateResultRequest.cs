using System;
using System.ComponentModel.DataAnnotations;

namespace RaceDay.Api.Dtos;

public class CreateResultRequest
{
    [Range(1, int.MaxValue)]
    public int EnrolmentId { get; set; }

    public TimeSpan? FinishTime { get; set; }

    [Range(1, 100000)]
    public int? Position { get; set; }

    [Required]
    [MaxLength(30)]
    public string ResultStatus { get; set; } = "Official";
}