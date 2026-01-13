using Tickets.Application.Common.Caching;
using Tickets.Application.Common.Localization;
using Tickets.Domain.Entity;
using Tickets.Domain.IRepository;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Tickets.Application.Command.User.Handlers
{
    public class UpdateUserCommandHandler : IRequestHandler<UpdateUserAsyncCommand, bool>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly IAppLocalizer _localizer;
        private readonly ICacheService _cache;

        public UpdateUserCommandHandler(
            UserManager<ApplicationUser> userManager,
            RoleManager<ApplicationRole> roleManager,
            IAppLocalizer localizer,
            ICacheService cache)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _localizer = localizer;
            _cache = cache;
        }

        public async Task<bool> Handle(UpdateUserAsyncCommand request, CancellationToken cancellationToken)
        {
            var dto = request.UpdateUserDto;
            if (dto == null)
                throw new ArgumentException(_localizer[LocalizationMessages.UserUpdateDataRequired]);

            var user = await _userManager.FindByIdAsync(dto.Id);
            if (user == null)
                throw new KeyNotFoundException(_localizer[LocalizationMessages.UserNotFound]);

            // Validate Email/Mobile uniqueness if changed
            if (!string.IsNullOrWhiteSpace(dto.Email) && dto.Email != user.Email)
            {
                if (await _userManager.Users.AnyAsync(u => u.Id != dto.Id && u.Email == dto.Email && !u.IsDeleted, cancellationToken))
                    throw new ArgumentException(_localizer[LocalizationMessages.EmailAlreadyInUse]);
            }

            if (!string.IsNullOrWhiteSpace(dto.Mobile) && dto.Mobile != user.PhoneNumber)
            {
                if (await _userManager.Users.AnyAsync(u => u.Id != dto.Id && u.PhoneNumber == dto.Mobile && !u.IsDeleted, cancellationToken))
                    throw new ArgumentException(_localizer[LocalizationMessages.MobileAlreadyInUse]);
            }

            user.FullName = dto.FullName ?? user.FullName;
            user.Email = dto.Email ?? user.Email;
            user.PhoneNumber = dto.Mobile ?? user.PhoneNumber;
            user.UserName = dto.Username ?? user.UserName;

            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
                throw new ApplicationException(_localizer[LocalizationMessages.FailedToUpdateUser]);

            if (!string.IsNullOrWhiteSpace(dto.Password))
            {
                var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
                var passwordResult = await _userManager.ResetPasswordAsync(user, resetToken, dto.Password);
                if (!passwordResult.Succeeded)
                    throw new ApplicationException(_localizer[LocalizationMessages.FailedToResetPassword]);
            }

            if (dto.RoleIds?.Any() == true)
            {
                var oldRoles = await _userManager.GetRolesAsync(user);
                await _userManager.RemoveFromRolesAsync(user, oldRoles);

                var roleNames = await _roleManager.Roles
                    .Where(r => dto.RoleIds.Contains(r.Id))
                    .Select(r => r.Name!)
                    .ToListAsync(cancellationToken);

                await _userManager.AddToRolesAsync(user, roleNames);
            }

            await _cache.RemoveAsync(CacheKeys.UsersAll, cancellationToken);
            await _cache.RemoveAsync(CacheKeys.UserById(dto.Id), cancellationToken);

            return true;
        }
    }
}
