using Tickets.Application.DTOs.Paymob;
using Tickets.Domain.Enums;

namespace Tickets.Application.Common.Interfaces
{
    public interface IPaymentService
    {
        Task<PaymentInitiationResult> InitiatePaymentAsync(Guid referenceId, string referenceType, decimal amount, string currency, string? userId, string? customerName = null, string? customerEmail = null, string? customerPhone = null);
        Task<bool> ProcessCallbackAsync(string hmac, string rawPayload);
        Task<PaymentStatusResponse> GetPaymentStatusAsync(Guid referenceId, string referenceType);
    }

    public class PaymentInitiationResult
    {
        public Guid OrderId { get; set; }
        public Guid PaymentAttemptId { get; set; }
        public string CheckoutUrl { get; set; } = null!;
        public PaymentStatus Status { get; set; }
    }

    public class PaymentStatusResponse
    {
        public Guid OrderId { get; set; }
        public PaymentStatus Status { get; set; }
        public string Message { get; set; } = null!;
    }
}
