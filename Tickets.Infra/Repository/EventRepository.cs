using Microsoft.EntityFrameworkCore;
using Tickets.Domain.Entity;
using Tickets.Domain.IRepository;
using Tickets.Infra.Data;

namespace Tickets.Infra.Repository
{
    public class EventRepository : BaseRepository<Event>, IEventRepository
    {
        private readonly ApplicationDbContext _context;

        public EventRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Event>> GetEventsByOwnerIdAsync(string ownerId)
        {
            return await _context.Events
                .Where(e => e.OwnerId == ownerId)
                .ToListAsync();
        }
    }
}
