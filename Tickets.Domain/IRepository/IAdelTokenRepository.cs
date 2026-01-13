
using Tickets.Domain.Entity;

namespace Tickets.Domain.IRepository
{
    public interface IAdelTokenRepository
    {
        Task AddAsync(string tokenValue, CancellationToken ct);
        Task<List<Tokens>> GetAllValuesAsync(CancellationToken ct);
    }
}
