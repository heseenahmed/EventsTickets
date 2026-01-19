using Tickets.Application.Common;
using Tickets.Application.Common.Caching;
using Tickets.Application.Common.Localization;
using Tickets.Application.Common.Interfaces;
using Tickets.Domain.Entity;
using Tickets.Domain.Enums;
using Tickets.Domain.IRepository;
using MediatR;
using Tickets.Application.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Tickets.Application.Command.User.Handlers
{
    public class RegisterCommandHandler : IRequestHandler<RegisterCommand, APIResponse<bool>>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IAppLocalizer _localizer;
        private readonly ICacheService _cache;
        private readonly IUnitOfWork _uow;

        public RegisterCommandHandler(
            UserManager<ApplicationUser> userManager,
            IAppLocalizer localizer,
            ICacheService cache,
            IUnitOfWork uow)
        {
            _userManager = userManager;
            _localizer = localizer;
            _cache = cache;
            _uow = uow;
        }

        public async Task<APIResponse<bool>> Handle(RegisterCommand request, CancellationToken cancellationToken)
        {
            var dto = request.userDto;
            if (dto == null)
                return APIResponse<bool>.Fail(StatusCodes.Status400BadRequest, new List<string> { _localizer[LocalizationMessages.RegistrationPayloadRequired] });

            // Business Validation
            var normalizedEmail = dto.Email.Trim().ToUpperInvariant();
            var emailExists = await _userManager.Users.AnyAsync(u => u.NormalizedEmail == normalizedEmail && !u.IsDeleted, cancellationToken);
            if (emailExists)
                return APIResponse<bool>.Fail(StatusCodes.Status409Conflict, new List<string> { _localizer[LocalizationMessages.EmailAlreadyInUse] });

            var mobileExists = await _userManager.Users.AnyAsync(u => u.PhoneNumber == dto.Mobile.Trim() && !u.IsDeleted, cancellationToken);
            if (mobileExists)
                return APIResponse<bool>.Fail(StatusCodes.Status409Conflict, new List<string> { _localizer[LocalizationMessages.MobileAlreadyInUse] });

            var user = new ApplicationUser
            {
                UserName = (dto.FullName.Replace(" ", "").Trim() + dto.Mobile.Trim()),
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
                var createResult = await _userManager.CreateAsync(user, dto.Password);
                if (!createResult.Succeeded)
                {
                    await _uow.RollbackAsync();
                    var errors = createResult.Errors.Select(e => e.Description).ToList();
                    return APIResponse<bool>.Fail(StatusCodes.Status400BadRequest, errors, _localizer[LocalizationMessages.FailedToCreateUser]);
                }

                await _uow.CommitAsync();
                await _cache.RemoveAsync(CacheKeys.UsersAll, cancellationToken);
                return APIResponse<bool>.Success(true, _localizer[LocalizationMessages.UserRegisteredSuccessfully]);
            }
            catch (Exception ex)
            {
                await _uow.RollbackAsync();
                return APIResponse<bool>.Exception(ex, _localizer[LocalizationMessages.RegistrationFailedInternally]);
            }
        }
    }
}
