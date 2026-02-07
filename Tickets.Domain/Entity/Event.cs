using System.ComponentModel.DataAnnotations;
using Tickets.Domain.Enums;

namespace Tickets.Domain.Entity
{
    public class Event : BaseEntity
    {
        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = null!;

        public string? Description { get; set; }

        [Required]
        [MaxLength(500)]
        public string Location { get; set; } = null!;

        public DateTime Date { get; set; }

        public decimal Price { get; set; }
        public decimal VisitorFee { get; set; }
        public int NumberOfVisitorsAllowed { get; set; }

        public int AvailableNumberOfVisitors { get; set; }

        public string? ImageUrl { get; set; }

        public string? EventDetails { get; set; }

        public string? TermsOfEntries { get; set; }

        public EventType Type { get; set; }

        // Event Owner relationship
        public string? OwnerId { get; set; }
        public virtual ApplicationUser? Owner { get; set; }

        public virtual ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
    }
}
