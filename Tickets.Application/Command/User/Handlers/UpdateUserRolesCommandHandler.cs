using FleetLinker.Application.Common.Caching;
using FleetLinker.Application.Common.Localization;
using FleetLinker.Application.DTOs.User;
using FleetLinker.Domain.Entity;
using FleetLinker.Domain.Models;
using FleetLinker.Domain.IRepository;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace FleetLinker.Application.Command.User.Handlers
{
    public class UpdateUserRolesCommandHandler : IRequestHandler<UpdateUserRolesCommand, UpdateUserRolesResult>
    {
        private readonly IUserRepository _userRepository;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IAppLocalizer _localizer;
        private readonly ICacheService _cache;

        public UpdateUserRolesCommandHandler(
            IUserRepository userRepository,
            UserManager<ApplicationUser> userManager,
            IAppLocalizer localizer,
            ICacheService cache)
        {
            _userRepository = userRepository;
            _userManager = userManager;
            _localizer = localizer;
            _cache = cache;
        }

        public async Task<UpdateUserRolesResult> Handle(UpdateUserRolesCommand request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.UserId))
                throw new ArgumentException(_localizer[LocalizationMessages.UserIdRequired]);

            if (request.RoleIds is null)
                throw new ArgumentException(_localizer[LocalizationMessages.RoleIdsCannotBeNull]);

            var targetUser = await _userManager.FindByIdAsync(request.UserId);
            if (targetUser == null)
                throw new KeyNotFoundException(_localizer[LocalizationMessages.TargetUserNotFound]);

            var result = await _userRepository.UpdateUserRolesAsync(request.UserId, request.RoleIds, cancellationToken);

            // ?? Roles affect authorization & listing
            await _cache.RemoveAsync(CacheKeys.UsersAll, cancellationToken);
            await _cache.RemoveAsync(
                CacheKeys.UserById(request.UserId),
                cancellationToken);

            return result;
        }
    }
}
