using Tickets.Domain.Entity;
using Tickets.Domain.Models;
namespace Tickets.Domain.IRepository
{
    public interface IRoleRepository 
    {
        public Task<IEnumerable<ApplicationRole>> GetRolesAsync();
        public Task<ApplicationRole?> GetRoleByIdAsync(string id);
        public Task<ApplicationRole?> GetRoleByNameAsync(string roleName);
        public Task<bool> AddRoleAsync(ApplicationRole Role);
        public Task<bool> UpdateRoleAsync(RoleDto Role);
        public Task<bool> DeleteRoleByNameAsync(string roleName);
        public Task<bool> DeleteRoleByIdAsync(string id);
        public  bool  IsRoleExistsAsync(string RoleName);
    }
}
