using Tickets.Domain.Entity;
using Tickets.Domain.IRepository;
using Tickets.Infra.Data;

namespace Tickets.Infra.Repository
{
    public class EventRepository : BaseRepository<Event>, IEventRepository
    {
        public EventRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}
