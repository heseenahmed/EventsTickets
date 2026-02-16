using System.Threading.Tasks;

namespace Tickets.Application.Common.Interfaces
{
    public interface IPaymobService
    {
        Task<string> GetAuthenticationTokenAsync();
        Task<int> CreateOrderAsync(string authToken, decimal amountInCents, string currency = "EGP");
        Task<string> GeneratePaymentKeyAsync(string authToken, int orderId, decimal amountInCents, string currency = "EGP");
    }
}
