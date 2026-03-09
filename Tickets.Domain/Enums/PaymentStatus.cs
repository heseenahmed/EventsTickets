namespace Tickets.Domain.Enums
{
    public enum PaymentStatus
    {
        Unpaid = 0,
        Pending = 1,
        Paid = 2,
        Failed = 3,
        Cancelled = 4,
        Expired = 5,
        Refunded = 6
    }
}
