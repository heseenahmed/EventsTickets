using System.Text.Json.Serialization;

namespace Tickets.Application.DTOs.Paymob
{
    public class PaymobIntentionRequest
    {
        [JsonPropertyName("amount")]
        public long Amount { get; set; } // Minor units

        [JsonPropertyName("currency")]
        public string Currency { get; set; } = "EGP";

        [JsonPropertyName("payment_methods")]
        public List<int> PaymentMethods { get; set; } = new();

        [JsonPropertyName("billing_data")]
        public PaymobBillingData BillingData { get; set; } = new();

        [JsonPropertyName("extras")]
        public Dictionary<string, string> Extras { get; set; } = new();

        [JsonPropertyName("special_reference")]
        public string? SpecialReference { get; set; }

        [JsonPropertyName("notification_url")]
        public string? NotificationUrl { get; set; }

        [JsonPropertyName("redirection_url")]
        public string? RedirectionUrl { get; set; }
    }

    public class PaymobBillingData
    {
        [JsonPropertyName("first_name")]
        public string FirstName { get; set; } = "NA";

        [JsonPropertyName("last_name")]
        public string LastName { get; set; } = "NA";

        [JsonPropertyName("email")]
        public string Email { get; set; } = "NA";

        [JsonPropertyName("phone_number")]
        public string PhoneNumber { get; set; } = "NA";

        [JsonPropertyName("apartment")]
        public string Apartment { get; set; } = "NA";

        [JsonPropertyName("floor")]
        public string Floor { get; set; } = "NA";

        [JsonPropertyName("street")]
        public string Street { get; set; } = "NA";

        [JsonPropertyName("building")]
        public string Building { get; set; } = "NA";

        [JsonPropertyName("city")]
        public string City { get; set; } = "NA";

        [JsonPropertyName("country")]
        public string Country { get; set; } = "EG";

        [JsonPropertyName("state")]
        public string State { get; set; } = "NA";
    }

    public class PaymobIntentionResponse
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = null!;

        [JsonPropertyName("client_secret")]
        public string ClientSecret { get; set; } = null!;

        [JsonPropertyName("payment_methods")]
        public List<PaymobPaymentMethodInfo> PaymentMethods { get; set; } = new();
    }

    public class PaymobPaymentMethodInfo
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("payment_method_type")]
        public string Type { get; set; } = null!;
    }

    public class PaymobTransactionResponse
    {
        [JsonPropertyName("id")]
        public long Id { get; set; }

        [JsonPropertyName("success")]
        public bool Success { get; set; }

        [JsonPropertyName("pending")]
        public bool Pending { get; set; }

        [JsonPropertyName("is_auth")]
        public bool IsAuth { get; set; }

        [JsonPropertyName("is_capture")]
        public bool IsCapture { get; set; }

        [JsonPropertyName("is_standalone_payment")]
        public bool IsStandalone { get; set; }

        [JsonPropertyName("is_voided")]
        public bool IsVoided { get; set; }

        [JsonPropertyName("is_refunded")]
        public bool IsRefunded { get; set; }

        [JsonPropertyName("amount_cents")]
        public long AmountCents { get; set; }

        [JsonPropertyName("currency")]
        public string Currency { get; set; } = null!;

        [JsonPropertyName("data.message")]
        public string? Message { get; set; }
    }
}
