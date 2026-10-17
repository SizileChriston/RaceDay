using System;
using System.ComponentModel.DataAnnotations;

namespace RaceDay.Api.Models;

public class Result
{
    public int ResultId { get; set; }

    public int EnrolmentId { get; set; }

    public TimeSpan? FinishTime { get; set; }

    [Range(1, 100000)]
    public int? Position { get; set; }

    [Required]
    [MaxLength(30)]
    public string ResultStatus { get; set; } = "Official";

    public DateTime RecordedAt { get; set; } = DateTime.UtcNow;

    public Enrolment? Enrolment { get; set; }
}