
using System;
using System.ComponentModel.DataAnnotations;

namespace RaceDay.Api.Dtos
{
    public class CreateEventRequest
    {
        [Required]
        [StringLength(100)]
        public string EventName { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        [Required]
        public DateTime? EventDate { get; set; }

        [Required]
        [StringLength(150)]
        public string Location { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string EventType { get; set; } = string.Empty;
    }
}
