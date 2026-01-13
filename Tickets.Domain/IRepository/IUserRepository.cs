using FleetLinker.Domain.Entity;
using FleetLinker.Domain.Models;

namespace FleetLinker.Domain.IRepository
{
    public interface IUserRepository
    {
        Task<UserInfoAPI?> GetUserInfoAsync(string id);
        Task<ApplicationUser?> GetByIdAsync(string id);
        Task<List<ApplicationUser>> GetAllAsync();
        Task<UpdateUserRolesResult> UpdateUserRolesAsync(string userId, IEnumerable<Guid> roleIds, CancellationToken ct = default);
    }
}