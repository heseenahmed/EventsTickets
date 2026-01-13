using Tickets.Domain.Entity;
using Tickets.Domain.Models;

namespace Tickets.Domain.IRepository
{
    public interface IUserRepository
    {
        Task<UserInfoAPI?> GetUserInfoAsync(string id);
        Task<ApplicationUser?> GetByIdAsync(string id);
        Task<List<ApplicationUser>> GetAllAsync();
        Task<UpdateUserRolesResult> UpdateUserRolesAsync(string userId, IEnumerable<Guid> roleIds, CancellationToken ct = default);
    }
}
