using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Tickets.Domain.Entity
{
    public class Booking : BaseEntity
    {
        public string? StudentId { get; set; }

        [ForeignKey("StudentId")]
        public virtual ApplicationUser? Student { get; set; }

        [Required]
        public Guid EventId { get; set; }

        [ForeignKey("EventId")]
        public virtual Event Event { get; set; } = null!;

        [Required]
        [MaxLength(200)]
        public string AttendeeName { get; set; } = null!;

        [Required]
        [EmailAddress]
        [MaxLength(200)]
        public string AttendeeEmail { get; set; } = null!;

        [Required]
        [Phone]
        [MaxLength(20)]
        public string AttendeePhone { get; set; } = null!;

        public string? AttendeeImageUrl { get; set; }

        public int NumberOfVisitors { get; set; }

        public decimal TotalPrice { get; set; }

        public bool IsPaid { get; set; }

        [Required]
        [MaxLength(500)]
        public string QrCodeData { get; set; } = null!;

        public int MaxEntries { get; set; } // 1 (attendee) + NumberOfVisitors

        public int CurrentEntries { get; set; }

        public DateTime BookingDate { get; set; } = DateTime.UtcNow;
    }
}
