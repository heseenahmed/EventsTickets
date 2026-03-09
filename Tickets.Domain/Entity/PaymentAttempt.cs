using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Tickets.Domain.Enums;

namespace Tickets.Domain.Entity
{
    public class PaymentAttempt : BaseEntity
    {
        [Required]
        public Guid OrderId { get; set; }

        [ForeignKey("OrderId")]
        public virtual Order Order { get; set; } = null!;

        public PaymentGateway Provider { get; set; } = PaymentGateway.Paymob;

        public decimal Amount { get; set; }

        [Required]
        [MaxLength(10)]
        public string Currency { get; set; } = "EGP";

        public PaymentAttemptStatus Status { get; set; } = PaymentAttemptStatus.Initiated;

        [MaxLength(200)]
        public string? MerchantOrderReference { get; set; }

        [MaxLength(200)]
        public string? PaymobIntentionId { get; set; }

        [MaxLength(200)]
        public string? PaymobTransactionId { get; set; }

        [MaxLength(200)]
        public string? PaymobOrderReference { get; set; }

        [MaxLength(50)]
        public string? PaymobPaymentMethod { get; set; }

        [MaxLength(1000)]
        public string? CheckoutUrl { get; set; }

        public string? LastGatewayStatus { get; set; }
        public string? FailureCode { get; set; }
        public string? FailureMessage { get; set; }

        public string? RawCreateResponseJson { get; set; }
        public string? RawWebhookJson { get; set; }

        public bool WebhookVerified { get; set; }
        public DateTime? WebhookVerifiedAt { get; set; }
        public DateTime? CallbackReceivedAt { get; set; }

        public bool IsFinal { get; set; }

        [MaxLength(200)]
        public string? IdempotencyKey { get; set; }

        public DateTime? ExpiresAt { get; set; }
    }
}
