using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using Tickets.Application.Common.Interfaces;
using Tickets.Application.Common.Models;

namespace Tickets.Infra.Repository
{
    public class PaymobWebhookVerifier : IPaymobWebhookVerifier
    {
        private readonly PaymobOptions _options;

        public PaymobWebhookVerifier(IOptions<PaymobOptions> options)
        {
            _options = options.Value;
        }

        public bool VerifyHmac(string payload, string hmac)
        {
            if (string.IsNullOrEmpty(hmac)) return false;

            var keyBytes = Encoding.UTF8.GetBytes(_options.HmacKey);
            var payloadBytes = Encoding.UTF8.GetBytes(payload);

            using var hmacAlgorithm = new HMACSHA256(keyBytes);
            var hashBytes = hmacAlgorithm.ComputeHash(payloadBytes);
            var calculatedHmac = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();

            return string.Equals(calculatedHmac, hmac, StringComparison.OrdinalIgnoreCase);
        }
    }
}
