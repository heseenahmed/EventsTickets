namespace Tickets.Application.DTOs.Checkout
{
    public class MyCheckoutDto
    {
        public Guid BookingId { get; set; }
        public Guid EventId { get; set; }
        public string EventName { get; set; } = null!;
        public decimal TotalAmount { get; set; }
        public int TotalVisitors { get; set; }
        public string PaymentStatus { get; set; } = null!;
        public DateTime CheckoutDate { get; set; }
    }
}
