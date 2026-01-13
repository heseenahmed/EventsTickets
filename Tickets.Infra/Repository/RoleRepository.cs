using FleetLinker.Application.Common.Localization;
using FleetLinker.Domain.Entity;
using FleetLinker.Domain.Models;
using FleetLinker.Domain.IRepository;
using FleetLinker.Infra.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace FleetLinker.Infra.Repository
{
    public class RoleRepository : IRoleRepository
    {
        #region Fields

        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly ApplicationDbContext _context;
        private readonly IAppLocalizer _Localizer;
        private static readonly Dictionary<string, string> RoleNameMappings = new()
        {
            { "Admin", "Admin" },
            { "Client", "Client" },
            { "Visitor", "Visitor" },
            { "Workshop", "Workshop" }
        };

        #endregion

        #region Constructor

        public RoleRepository(RoleManager<ApplicationRole> roleManager, ApplicationDbContext context , IAppLocalizer localizer)
        {
            _roleManager = roleManager;
            _context = context;
            _Localizer = localizer;
        }

        #endregion

        #region Add Role

        public async Task<bool> AddRoleAsync(ApplicationRole role)
        {
            if (role is null) throw new ArgumentNullException(nameof(role));
            if (string.IsNullOrWhiteSpace(role.Name)) throw new ArgumentException(_Localizer[LocalizationMessages.RoleNameRequired]);
            if (IsRoleExistsAsync(role.Name)) return true;

            role.NormalizedName = role.Name.ToUpperInvariant();
            var result = await _roleManager.CreateAsync(role);

            if (!result.Succeeded)
                throw new ApplicationException(_Localizer[LocalizationMessages.ErrorAddingRole]);

            return true;
        }

        #endregion

        #region Delete Role

        public async Task<bool> DeleteRoleByIdAsync(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException(_Localizer[LocalizationMessages.RoleIdsCannotBeNull]);

            var role = await _roleManager.FindByIdAsync(id);
            if (role == null) throw new KeyNotFoundException(_Localizer[LocalizationMessages.RoleNotFound]);

            var result = await _roleManager.DeleteAsync(role);
            if (!result.Succeeded)
                throw new ApplicationException(_Localizer[LocalizationMessages.ErrorDeleteRole]);

            return true;
        }

        public async Task<bool> DeleteRoleByNameAsync(string roleName)
        {
            if (string.IsNullOrWhiteSpace(roleName)) throw new ArgumentException(_Localizer[LocalizationMessages.RoleNameRequired]);

            var role = await _roleManager.FindByNameAsync(roleName);
            if (role == null) throw new KeyNotFoundException(_Localizer[LocalizationMessages.RoleNotFound]);

            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var userRoles = await _context.UserRoles
                    .Where(ur => ur.RoleId == role.Id)
                    .ToListAsync();

                if (userRoles.Count > 0)
                {
                    _context.UserRoles.RemoveRange(userRoles);
                    await _context.SaveChangesAsync();
                }

                var result = await _roleManager.DeleteAsync(role);
                if (!result.Succeeded)
                    throw new ApplicationException(_Localizer[LocalizationMessages.ErrorDeleteRole]);

                await transaction.CommitAsync();
                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        #endregion

        #region Get Roles

        public async Task<ApplicationRole?> GetRoleByIdAsync(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException(_Localizer[LocalizationMessages.RoleIdsCannotBeNull]);
            return await _roleManager.FindByIdAsync(id);
        }

        public async Task<ApplicationRole?> GetRoleByNameAsync(string roleName)
        {
            if (string.IsNullOrWhiteSpace(roleName)) throw new ArgumentException(_Localizer[LocalizationMessages.RoleNameRequired]);
            return await _roleManager.FindByNameAsync(roleName);
        }

        public async Task<IEnumerable<ApplicationRole>> GetRolesAsync()
        {
            var roles = await _roleManager.Roles
                                .AsNoTracking()
                                .OrderByDescending(r => r.Id)
                                .ToListAsync();

            foreach (var role in roles)
            {
                if (RoleNameMappings.TryGetValue(role.Name ?? string.Empty, out var friendlyName))
                {
                    role.Name = friendlyName;
                }
            }

            return roles;
        }

        #endregion

        #region Role Validation

        public bool IsRoleExistsAsync(string roleName)
        {
            if (string.IsNullOrWhiteSpace(roleName)) throw new ArgumentException(_Localizer[LocalizationMessages.RoleNameRequired]);
            return _roleManager.Roles.Any(e => e.Name != null && e.Name.ToLower() == roleName.ToLower());
        }

        #endregion

        #region Update Role

        public async Task<bool> UpdateRoleAsync(RoleDto roleDto)
        {
            if (roleDto is null) throw new ArgumentNullException(nameof(roleDto));
            if (string.IsNullOrWhiteSpace(roleDto.Name)) throw new ArgumentException(_Localizer[LocalizationMessages.RoleNameRequired]);

            var role = await _context.Roles
                .FirstOrDefaultAsync(r => r.Name != null && r.Name.ToLower() == roleDto.Name.ToLower());

            if (role == null) throw new KeyNotFoundException(_Localizer[LocalizationMessages.RoleNotFound]);

            role.Name = roleDto.Name;
            role.NormalizedName = roleDto.Name.ToUpperInvariant();

            var changed = await _context.SaveChangesAsync();
            return changed > 0;
        }

        #endregion
    }
}
