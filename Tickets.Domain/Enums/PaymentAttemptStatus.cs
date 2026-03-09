namespace Tickets.Domain.Enums
{
    public enum PaymentAttemptStatus
    {
        Initiated = 0,
        Pending = 1,
        RequiresAction = 2,
        Authorized = 3,
        Captured = 4, // Final success for some flows
        Success = 5,
        Failed = 6,
        Cancelled = 7,
        Expired = 8,
        Refunded = 9
    }
}
