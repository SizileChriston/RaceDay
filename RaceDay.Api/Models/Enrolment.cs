using System;
using System.ComponentModel.DataAnnotations;

namespace RaceDay.Api.Models;

public class Enrolment
{
    [Key]
    public int EnrolmentId { get; set; }

    public int UserId { get; set; }

    public int CategoryId { get; set; }

    public DateTime EnrolmentDate { get; set; } = DateTime.UtcNow;

    [Required]
    [MaxLength(30)]
    public string Status { get; set; } = "Confirmed";

    public User? User { get; set; }

    public Category? Category { get; set; }

    public Result? Result { get; set; }
}