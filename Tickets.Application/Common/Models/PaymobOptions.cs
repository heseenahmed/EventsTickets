namespace Tickets.Application.Common.Models
{
    public class PaymobOptions
    {
        public const string SectionName = "PaymentSettings:Paymob";

        public string ApiKey { get; set; } = null!;
        public string SecretKey { get; set; } = null!;
        public string PublicKey { get; set; } = null!;
        public string HmacKey { get; set; } = null!;
        public string BaseUrl { get; set; } = "https://gcc.paymob.com/api/"; // Default to Unified API base
        public string ReturnUrl { get; set; } = null!;
        public string WebhookUrl { get; set; } = null!;
        public int IntegrationId { get; set; }
        public bool IsTestMode { get; set; } = true;
    }
}
