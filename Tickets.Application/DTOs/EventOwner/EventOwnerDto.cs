namespace Tickets.Application.DTOs.EventOwner
{
    public class EventOwnerDto
    {
        public string Id { get; set; } = null!;
        public string FullName { get; set; } = null!;
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public int OwnedEventsCount { get; set; }
    }

    public class AssignEventToOwnerRequest
    {
        public string UserId { get; set; } = null!;
        public Guid EventId { get; set; }
    }
}
