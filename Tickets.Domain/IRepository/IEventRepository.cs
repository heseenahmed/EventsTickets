using Tickets.Domain.Entity;

namespace Tickets.Domain.IRepository
{
    public interface IEventRepository : IBaseRepository<Event>
    {
        Task<IEnumerable<Event>> GetEventsByOwnerIdAsync(string ownerId);
    }
}
