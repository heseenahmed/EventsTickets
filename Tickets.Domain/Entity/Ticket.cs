using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Tickets.Domain.Entity
{
    public class Ticket : BaseEntity
    {
        [Required]
        public Guid EventId { get; set; }

        [ForeignKey("EventId")]
        public virtual Event Event { get; set; } = null!;

        public string? StudentId { get; set; } // Optional user linking

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

        [MaxLength(200)]
        public string? Faculty { get; set; } // كليه

        [MaxLength(200)]
        public string? Department { get; set; } // قسم

        [MaxLength(100)]
        public string? Year { get; set; } // فرقه

        public int VisitorCount { get; set; }

        public int MaxScans { get; set; } // VisitorCount + 1

        public int ScannedCount { get; set; }
        public decimal TotalPrice { get; set; }
        public bool IsPaid { get; set; }
        [MaxLength(500)]
        public string? QrToken { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
