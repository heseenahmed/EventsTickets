
namespace Tickets.Application.Common.Interfaces
{
    public interface IUnitOfWork
    {
        Task BeginTransactionAsync();
        Task CommitAsync();
        Task RollbackAsync();
        Task<T> ExecuteAsync<T>(Func<Task<T>> action);
    }
}
