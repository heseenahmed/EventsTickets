
namespace FleetLinker.Domain.IRepository
{
    public interface INoteRepository
    {
        Task<int> AddAsync(string title, string details, CancellationToken ct);

    }
}
