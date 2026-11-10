using System.ComponentModel.DataAnnotations;

namespace RaceDay.Mvc.Models;

public sealed class EventViewModel
{
    public int EventId { get; set; }
    [Required, StringLength(100)] public string EventName { get; set; } = "";
    [StringLength(500)] public string? Description { get; set; }
    [Required] public DateTime EventDate { get; set; } = DateTime.Today.AddDays(7);
    [Required, StringLength(150)] public string Location { get; set; } = "";
    [Required] public string EventType { get; set; } = "Running";
    public string? BannerImageUrl { get; set; }
    public int TotalEnrolments { get; set; }
}

public sealed class CategoryViewModel
{
    public int CategoryId { get; set; }
    public int EventId { get; set; }
    public string CategoryName { get; set; } = "";
    public decimal DistanceKm { get; set; }
    public int MaxParticipants { get; set; }
    public decimal EntryFee { get; set; }
}

public sealed class EnrolmentViewModel
{
    public int EnrolmentId { get; set; }
    public int UserId { get; set; }
    public int CategoryId { get; set; }
    public DateTime EnrolmentDate { get; set; }
    public string Status { get; set; } = "";
    public string? CategoryName { get; set; }
    public decimal? DistanceKm { get; set; }
    public string? EventName { get; set; }
    public DateTime? EventDate { get; set; }
    public string? ParticipantName { get; set; }
    public CategoryInfo? Category { get; set; }
    public ParticipantInfo? Participant { get; set; }
    public sealed class CategoryInfo
    {
        public string? CategoryName { get; set; }
        public decimal DistanceKm { get; set; }
        public EventInfo? Event { get; set; }
    }
    public sealed class EventInfo
    {
        public int EventId { get; set; }
        public string? EventName { get; set; }
        public DateTime EventDate { get; set; }
        public string? Location { get; set; }
    }
    public sealed class ParticipantInfo
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Email { get; set; }
    }
}

public sealed class ResultViewModel
{
    public int ResultId { get; set; }
    public int EnrolmentId { get; set; }
    public TimeSpan? FinishTime { get; set; }
    public int? Position { get; set; }
    public int TotalFinishers { get; set; }
    public string ResultStatus { get; set; } = "";
    public DateTime RecordedAt { get; set; }
    public CategoryDetail? Category { get; set; }
    public sealed class CategoryDetail
    {
        public string? CategoryName { get; set; }
        public decimal DistanceKm { get; set; }
        public EventDetail? Event { get; set; }
    }
    public sealed class EventDetail
    {
        public int EventId { get; set; }
        public string? EventName { get; set; }
        public DateTime EventDate { get; set; }
        public string? Location { get; set; }
    }
}

public sealed class AccountViewModel
{
    [Required, EmailAddress] public string Email { get; set; } = "";
    [Required, DataType(DataType.Password)] public string Password { get; set; } = "";
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
}

public sealed class ProfileViewModel
{
    public int UserId { get; set; }
    [Required, StringLength(50)] public string FirstName { get; set; } = "";
    [Required, StringLength(50)] public string LastName { get; set; } = "";
    [Required, EmailAddress] public string Email { get; set; } = "";
    public string Role { get; set; } = "";
    public string? ProfilePictureUrl { get; set; }
}

public sealed class ResultEntryViewModel
{
    [Range(1, int.MaxValue)] public int EnrolmentId { get; set; }
    [Required] public string FinishTime { get; set; } = "00:00:00";
    [Range(1, 100000)] public int Position { get; set; }
    [Required] public string ResultStatus { get; set; } = "Official";
}
