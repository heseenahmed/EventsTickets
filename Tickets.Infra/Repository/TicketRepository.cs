using Microsoft.EntityFrameworkCore;
using Tickets.Domain.Entity;
using Tickets.Domain.IRepository;
using Tickets.Infra.Data;

namespace Tickets.Infra.Repository
{
    public class TicketRepository : BaseRepository<Ticket>, ITicketRepository
    {
        private readonly ApplicationDbContext _context;

        public TicketRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<Ticket?> GetByQrTokenAsync(string qrToken, CancellationToken ct)
        {
            return await _context.Tickets
                .Include(t => t.Event)
                .FirstOrDefaultAsync(t => t.QrToken == qrToken, ct);
        }
        public async Task<IEnumerable<Ticket>> GetByEventIdAsync(Guid eventId, CancellationToken ct)
        {
            return await _context.Tickets
                .Include(t => t.Event)
                .Where(t => t.EventId == eventId)
                .ToListAsync(ct);
        }
    }
}
