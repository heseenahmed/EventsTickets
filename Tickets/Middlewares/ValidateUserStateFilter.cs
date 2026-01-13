using Tickets.Domain.Entity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Tickets.API.Middlewares
{
    public class ValidateUserStateFilter : IAsyncAuthorizationFilter
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public ValidateUserStateFilter(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            var userId = context.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier)
                         ?? context.HttpContext.User.FindFirstValue(JwtRegisteredClaimNames.Sub);

            if (string.IsNullOrEmpty(userId))
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            var user = await _userManager.FindByIdAsync(userId);

            if (user == null || !user.IsActive || user.IsDeleted)
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            var tokenStamp = context.HttpContext.User.FindFirst("security_stamp")?.Value;

            if (tokenStamp != user.SecurityStamp)
            {
                context.Result = new UnauthorizedResult();
            }
        }
    }

}
