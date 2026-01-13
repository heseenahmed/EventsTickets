using Tickets.Application.Common;
using Tickets.Application.Common.Caching;
using Tickets.Application.Common.Localization;
using Tickets.Application.Common.Interfaces;
using Tickets.Domain.Entity;
using Tickets.Domain.Enums;
using Tickets.Domain.IRepository;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Tickets.Application.Command.User.Handlers
{
    public class RegisterCommandHandler : IRequestHandler<RegisterCommand, bool>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly IAppLocalizer _localizer;
        private readonly ICacheService _cache;
        private readonly IUnitOfWork _uow;

        public RegisterCommandHandler(
            UserManager<ApplicationUser> userManager,
            RoleManager<ApplicationRole> roleManager,
            IAppLocalizer localizer,
            ICacheService cache,
            IUnitOfWork uow)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _localizer = localizer;
            _cache = cache;
            _uow = uow;
        }

        public async Task<bool> Handle(RegisterCommand request, CancellationToken cancellationToken)
        {
            var dto = request.userDto;
            if (dto == null)
                throw new ArgumentException(_localizer[LocalizationMessages.RegistrationPayloadRequired]);

            // Business Validation
            var normalizedEmail = dto.Email.Trim().ToUpperInvariant();
            var emailExists = await _userManager.Users.AnyAsync(u => u.NormalizedEmail == normalizedEmail && !u.IsDeleted, cancellationToken);
            if (emailExists)
                throw new ArgumentException(_localizer[LocalizationMessages.EmailAlreadyInUse]);

            var user = new ApplicationUser
            {
                UserName = string.IsNullOrWhiteSpace(dto.Username)
                    ? dto.FullName.Replace(" ", "").Trim()
                    : dto.Username.Trim(),
                FullName = dto.FullName.Trim(),
                Email = dto.Email.Trim(),
                NormalizedEmail = normalizedEmail,
                PhoneNumber = dto.Mobile.Trim(),
                IsActive = true,
                IsDeleted = false,
                RefreshToken = TokenGenerator.GenerateRefreshToken(),
                RefreshTokenExpiryUTC = DateTime.UtcNow.AddDays(7),
            };

            await _uow.BeginTransactionAsync();
            try
            {
                var createResult = await _userManager.CreateAsync(user);
                if (!createResult.Succeeded)
                    throw new ApplicationException(_localizer[LocalizationMessages.FailedToCreateUser]);

                if (dto.RoleIds?.Any() == true)
                {
                    foreach (var roleId in dto.RoleIds)
                    {
                        var role = await _roleManager.FindByIdAsync(roleId.ToString());
                        if (role == null)
                            throw new KeyNotFoundException(_localizer[LocalizationMessages.RoleNotFound]);

                        var addRoleResult = await _userManager.AddToRoleAsync(user, role.Name!);
                        if (!addRoleResult.Succeeded)
                            throw new ApplicationException(_localizer[LocalizationMessages.FailedToAssignRoles]);
                    }
                }

                var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
                var password = !string.IsNullOrWhiteSpace(dto.Password) ? dto.Password : dto.Mobile;
                var passwordResult = await _userManager.ResetPasswordAsync(user, resetToken, password);

                if (!passwordResult.Succeeded)
                    throw new ApplicationException(_localizer[LocalizationMessages.FailedToResetPassword]);

                await _uow.CommitAsync();
                await _cache.RemoveAsync(CacheKeys.UsersAll, cancellationToken);
                return true;
            }
            catch (Exception)
            {
                await _uow.RollbackAsync();
                throw;
            }
        }
    }
}
