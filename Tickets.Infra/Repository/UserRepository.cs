using Tickets.Domain.Entity;
using Tickets.Domain.Models;
using Tickets.Domain.IRepository;
using Tickets.Infra.Data;
using Tickets.Application.Common;
using Microsoft.EntityFrameworkCore;
using Tickets.Application.Common.Localization;
using System.Data;

namespace Tickets.Infra.Repository
{
    public class UserRepository : IUserRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly IAppLocalizer _Localizer;

        public UserRepository(ApplicationDbContext context, IAppLocalizer localizer)
        {
            _context = context;
            _Localizer = localizer;
        }

        public async Task<UserInfoAPI?> GetUserInfoAsync(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentException(_Localizer[LocalizationMessages.UserIdRequired]);

            return await _context.Users
                .Where(e => e.Id == id && e.IsActive && !e.IsDeleted)
                .Select(s => new UserInfoAPI
                {
                    FullName = s.FullName,
                    Mobile = s.PhoneNumber,
                    Id = s.Id
                })
                .FirstOrDefaultAsync();
        }

        public async Task<ApplicationUser?> GetByIdAsync(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentException(_Localizer[LocalizationMessages.UserIdRequired]);

            return await _context.Users
                .Include(x => x.UserRoles).ThenInclude(x => x.Role)
                .FirstOrDefaultAsync(e => e.Id == id);
        }

        public async Task<List<ApplicationUser>> GetAllAsync()
        {
            return await _context.Users
                .Include(x => x.UserRoles).ThenInclude(x => x.Role)
                .Where(x => !x.IsDeleted)
                .ToListAsync();
        }

        public async Task<UpdateUserRolesResult> UpdateUserRolesAsync(string userId, IEnumerable<Guid> roleIds, CancellationToken ct = default)
        {
            var user = await _context.Users
                .Include(u => u.UserRoles)
                .FirstOrDefaultAsync(u => u.Id == userId, ct);
            if (user is null)
                throw new KeyNotFoundException(_Localizer[LocalizationMessages.UserNotFound]);

            var targetRoleIdStrings = roleIds?.Select(g => g.ToString()).ToHashSet(StringComparer.OrdinalIgnoreCase)
                                   ?? new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            var rolesInDb = await _context.Roles
                .Where(r => targetRoleIdStrings.Contains(r.Id))
                .Select(r => new { r.Id, r.Name })
                .ToListAsync(ct);

            var foundIds = rolesInDb.Select(r => r.Id).ToHashSet(StringComparer.OrdinalIgnoreCase);
            var missingIds = targetRoleIdStrings.Except(foundIds, StringComparer.OrdinalIgnoreCase).ToList();
            if (missingIds.Count > 0)
                throw new ArgumentException(_Localizer[LocalizationMessages.SomeRolesNotExist] + $" {string.Join(", ", missingIds)}");

            var currentRoleIds = user.UserRoles.Select(ur => ur.RoleId).ToHashSet(StringComparer.OrdinalIgnoreCase);
            var toAdd = targetRoleIdStrings.Except(currentRoleIds, StringComparer.OrdinalIgnoreCase).ToList();
            var toRemove = currentRoleIds.Except(targetRoleIdStrings, StringComparer.OrdinalIgnoreCase).ToList();
            var kept = currentRoleIds.Intersect(targetRoleIdStrings, StringComparer.OrdinalIgnoreCase).ToList();

            using var tx = await _context.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted, ct);
            try
            {
                if (toRemove.Count > 0)
                {
                    var removeRows = await _context.UserRoles
                        .Where(ur => ur.UserId == userId && toRemove.Contains(ur.RoleId))
                        .ToListAsync(ct);
                    _context.UserRoles.RemoveRange(removeRows);
                }
                if (toAdd.Count > 0)
                {
                    var addRows = toAdd.Select(rid => new ApplicationUserRole
                    {
                        UserId = userId,
                        RoleId = rid
                    });
                    await _context.UserRoles.AddRangeAsync(addRows, ct);
                }
                await _context.SaveChangesAsync(ct);
                await tx.CommitAsync(ct);
            }
            catch
            {
                await tx.RollbackAsync(ct);
                throw;
            }

            var finalRoles = await (from ur in _context.UserRoles.AsNoTracking()
                                    join r in _context.Roles.AsNoTracking() on ur.RoleId equals r.Id
                                    where ur.UserId == userId
                                    select new RoleSummaryDto { Id = r.Id, Name = r.Name! })
                                   .OrderBy(x => x.Name)
                                   .ToListAsync(ct);

            return new UpdateUserRolesResult
            {
                UserId = userId,
                AddedCount = toAdd.Count,
                RemovedCount = toRemove.Count,
                KeptCount = kept.Count,
                FinalRoles = finalRoles
            };
        }

        // Methods removed and moved to Handlers (via UserManager directly):
        // RegisterAsync, UpdateUserAsync, SwitchUserActiveAsync, SoftDeleteUserAsync
        // Since these are wrapper around UserManager, we prefer calling UserManager in Handlers
        // to stay closer to Identity framework and allow Handlers to orchestrate cross-cutting concerns.
    }
}
