using Tickets.Application.DTOs.Paymob;

namespace Tickets.Application.Common.Interfaces
{
    public interface IPaymobClient
    {
        Task<PaymobIntentionResponse> CreateIntentionAsync(PaymobIntentionRequest request);
        Task<PaymobTransactionResponse> GetTransactionAsync(string transactionId);
    }

    public interface IPaymobWebhookVerifier
    {
        bool VerifyHmac(string payload, string hmac);
    }
}
