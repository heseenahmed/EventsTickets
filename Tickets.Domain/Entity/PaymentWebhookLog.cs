using System.ComponentModel.DataAnnotations;

namespace Tickets.Domain.Entity
{
    public class PaymentWebhookLog : BaseEntity
    {
        [Required]
        [MaxLength(50)]
        public string Provider { get; set; } = "Paymob";

        [MaxLength(100)]
        public string? EventType { get; set; }

        [MaxLength(200)]
        public string? ExternalEventId { get; set; }

        [Required]
        public string Payload { get; set; } = null!;

        public string? PayloadHash { get; set; }

        public string? Headers { get; set; }

        public string? SignatureOrHmac { get; set; }

        public bool IsVerified { get; set; }

        [MaxLength(50)]
        public string? ProcessingStatus { get; set; }

        public string? ProcessingError { get; set; }

        public int RetryCount { get; set; }

        public DateTime? ProcessedAt { get; set; }
    }
}
