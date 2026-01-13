using Tickets.Application.Common.Caching;
using Tickets.Application.Common.Localization;
using Tickets.Domain.Entity;
using Tickets.Domain.IRepository;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Tickets.Application.Command.User.Handlers
{
    public class SwitchUserActiveCommandHandler : IRequestHandler<SwitchUserActiveCommand, bool>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IAppLocalizer _localizer;
        private readonly ICacheService _cache;

        public SwitchUserActiveCommandHandler(
            UserManager<ApplicationUser> userManager,
            IAppLocalizer localizer,
            ICacheService cache)
        {
            _userManager = userManager;
            _localizer = localizer;
            _cache = cache;
        }

        public async Task<bool> Handle(SwitchUserActiveCommand request, CancellationToken cancellationToken)
        {
            if (request.Id == Guid.Empty)
                throw new ArgumentException(_localizer[LocalizationMessages.UserIdRequired]);

            var user = await _userManager.FindByIdAsync(request.Id.ToString());
            if (user == null)
                throw new KeyNotFoundException(_localizer[LocalizationMessages.UserNotFound]);

            user.IsActive = !user.IsActive;
            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
                throw new ApplicationException(_localizer[LocalizationMessages.FailedToUpdateUser]);

            // Invalidate tokens
            await _userManager.UpdateSecurityStampAsync(user);

            // ?? Status affects list + details
            await _cache.RemoveAsync(CacheKeys.UsersAll, cancellationToken);
            await _cache.RemoveAsync(
                CacheKeys.UserById(request.Id.ToString()),
                cancellationToken);

            return user.IsActive;
        }
    }
}
