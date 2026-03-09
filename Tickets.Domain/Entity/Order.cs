using System.ComponentModel.DataAnnotations;
using Tickets.Domain.Enums;

namespace Tickets.Domain.Entity
{
    public class Order : BaseEntity
    {
        public string? UserId { get; set; }

        [Required]
        public Guid ReferenceId { get; set; }

        [Required]
        [MaxLength(100)]
        public string ReferenceType { get; set; } = null!; // "Booking", "Ticket", etc.

        public decimal Amount { get; set; }

        [Required]
        [MaxLength(10)]
        public string Currency { get; set; } = "EGP";

        public PaymentStatus Status { get; set; } = PaymentStatus.Unpaid;

        public DateTime? PaidAt { get; set; }
        public DateTime? CancelledAt { get; set; }

        public virtual ICollection<PaymentAttempt> PaymentAttempts { get; set; } = new List<PaymentAttempt>();
    }
}
