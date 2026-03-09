using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Tickets.Application.Common.Interfaces;
using Tickets.Application.Common.Models;
using Tickets.Application.DTOs.Paymob;

namespace Tickets.Infra.Repository
{
    public class PaymobClient : IPaymobClient
    {
        private readonly HttpClient _httpClient;
        private readonly PaymobOptions _options;
        private readonly ILogger<PaymobClient> _logger;
        private readonly string _baseUrl;

        public PaymobClient(HttpClient httpClient, IOptions<PaymobOptions> options, ILogger<PaymobClient> logger)
        {
            _httpClient = httpClient;
            _options = options.Value;
            _logger = logger;

            // Ensure BaseUrl ends with /
            _baseUrl = _options.BaseUrl.TrimEnd('/') + "/";

            _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", "Token " + _options.SecretKey);
        }

        public async Task<PaymobIntentionResponse> CreateIntentionAsync(PaymobIntentionRequest request)
        {
            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Build full URL explicitly to avoid HttpClient BaseAddress issues
            var fullUrl = _baseUrl + "v1/intention/";
            _logger.LogInformation("Calling Paymob Intention API at: {Url}", fullUrl);

            var response = await _httpClient.PostAsync(fullUrl, content);
            var responseString = await response.Content.ReadAsStringAsync();

            _logger.LogInformation("Paymob Response Status: {StatusCode}, Body: {Body}", response.StatusCode, responseString);

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Paymob Intention API failed: {responseString}");
            }

            return JsonSerializer.Deserialize<PaymobIntentionResponse>(responseString) 
                   ?? throw new Exception("Failed to deserialize Paymob Intention response.");
        }

        public async Task<PaymobTransactionResponse> GetTransactionAsync(string transactionId)
        {
            var fullUrl = _baseUrl + $"v1/transaction/{transactionId}";
            var response = await _httpClient.GetAsync(fullUrl);
            var responseString = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Paymob Transaction API failed: {responseString}");
            }

            return JsonSerializer.Deserialize<PaymobTransactionResponse>(responseString)
                   ?? throw new Exception("Failed to deserialize Paymob Transaction response.");
        }
    }
}

