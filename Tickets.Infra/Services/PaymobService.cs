using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Tickets.Application.Common.Interfaces;
using Tickets.Application.Common.Options;

namespace Tickets.Infra.Services
{
    public class PaymobService : IPaymobService
    {
        private readonly HttpClient _httpClient;
        private readonly PaymobOptions _options;

        public PaymobService(HttpClient httpClient, IOptions<PaymobOptions> options)
        {
            _httpClient = httpClient;
            _options = options.Value;
        }

        public async Task<string> GetAuthenticationTokenAsync()
        {
            var requestBody = new { api_key = _options.ApiKey };
            var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

            var baseUrl = _options.BaseUrl.TrimEnd('/');
            var response = await _httpClient.PostAsync($"{baseUrl}/api/auth/tokens", content);
            response.EnsureSuccessStatusCode();

            var responseString = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(responseString);
            return doc.RootElement.GetProperty("token").GetString();
        }

        public async Task<int> CreateOrderAsync(string authToken, decimal amountInCents, string currency = "EGP")
        {
            var requestBody = new
            {
                auth_token = authToken,
                delivery_needed = false,
                amount_cents = (long)amountInCents,
                currency = currency,
                items = new object[] { }
            };

            var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

            var baseUrl = _options.BaseUrl.TrimEnd('/');
            var response = await _httpClient.PostAsync($"{baseUrl}/api/acceptance/orders", content);
            response.EnsureSuccessStatusCode();

            var responseString = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(responseString);
            return doc.RootElement.GetProperty("id").GetInt32();
        }

        public async Task<string> GeneratePaymentKeyAsync(string authToken, int orderId, decimal amountInCents, string currency = "EGP")
        {
            var requestBody = new
            {
                auth_token = authToken,
                amount_cents = (long)amountInCents,
                expiration = 3600,
                order_id = orderId.ToString(),
                billing_data = new
                {
                    apartment = "NA",
                    email = "clinet@example.com",
                    floor = "NA",
                    first_name = "Clinet",
                    street = "NA",
                    building = "NA",
                    phone_number = "+201000000000",
                    shipping_method = "NA",
                    postal_code = "NA",
                    city = "NA",
                    country = "NA",
                    last_name = "User",
                    state = "NA"
                },
                currency = currency,
                integration_id = int.Parse(_options.IntegrationId)
            };

            var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

            var baseUrl = _options.BaseUrl.TrimEnd('/');
            var response = await _httpClient.PostAsync($"{baseUrl}/api/acceptance/payment_keys", content);
            response.EnsureSuccessStatusCode();

            var responseString = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(responseString);
            return doc.RootElement.GetProperty("token").GetString();
        }
    }
}
