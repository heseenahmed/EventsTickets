namespace Tickets.Application.DTOs.Checkout
{
    public class AttendeeDto
    {
        public Guid Id { get; set; }
        public string? FullName { get; set; }
        public string? PhotoUrl { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? Companions { get; set; }
        public string? TotalPrice { get; set; }
        public string? Status { get; set; }
        public string? Attendance { get; set; }
    }
}
