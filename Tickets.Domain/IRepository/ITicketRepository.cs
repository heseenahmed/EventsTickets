using Tickets.Domain.Entity;

namespace Tickets.Domain.IRepository
{
    public interface ITicketRepository : IBaseRepository<Ticket>
    {
        Task<Ticket?> GetByQrTokenAsync(string qrToken, CancellationToken ct);
    }
}
