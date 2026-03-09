namespace Tickets.Application.DTOs.Event
{
    public class EventCheckoutResponseDto
    {
        public Guid BookingId { get; set; }
        public string CheckoutUrl { get; set; } = null!;
        public bool IsNewBooking { get; set; }
        public string Message { get; set; } = null!;
    }
}
