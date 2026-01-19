using Tickets.Domain.Entity;

namespace Tickets.Domain.IRepository
{
    public interface IBookingRepository : IBaseRepository<Booking>
    {
        Task<Booking?> GetByQrCodeAsync(string qrCodeData, CancellationToken ct);
        Task<Booking?> GetByStudentIdAsync(string studentId, CancellationToken ct);
    }
}
