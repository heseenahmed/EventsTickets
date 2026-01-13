using Tickets.Application.Common.Caching;
using Tickets.Application.Common.Localization;
using Tickets.Domain.Entity;
using Tickets.Domain.IRepository;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Tickets.Application.Command.User.Handlers
{
    public class DeleteUserCommandHandler : IRequestHandler<DeleteUserCommand, bool>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IAppLocalizer _localizer;
        private readonly ICacheService _cache;

        public DeleteUserCommandHandler(
            UserManager<ApplicationUser> userManager,
            IAppLocalizer localizer,
            ICacheService cache)
        {
            _userManager = userManager;
            _localizer = localizer;
            _cache = cache;
        }

        public async Task<bool> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.UserId))
                throw new ArgumentException(_localizer[LocalizationMessages.UserIdRequired]);

            var user = await _userManager.FindByIdAsync(request.UserId);
            if (user == null)
                throw new KeyNotFoundException(_localizer[LocalizationMessages.UserNotFound]);

            if (user.IsDeleted)
                throw new KeyNotFoundException(_localizer[LocalizationMessages.UserAlreadyDeleted]);

            user.IsDeleted = true;
            user.IsActive = false;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
                throw new ApplicationException(_localizer[LocalizationMessages.FailedToDeleteUser]);

            // ?? Invalidate all tokens immediately
            await _userManager.UpdateSecurityStampAsync(user);

            // ?? Remove from cache
            await _cache.RemoveAsync(CacheKeys.UsersAll, cancellationToken);
            await _cache.RemoveAsync(
                CacheKeys.UserById(request.UserId),
                cancellationToken);
            return true;
        }
    }
}
