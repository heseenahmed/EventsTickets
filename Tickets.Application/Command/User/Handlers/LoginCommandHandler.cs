using Tickets.Application.Common;
using Tickets.Application.Common.Localization;
using Tickets.Application.DTOs;
using Tickets.Application.DTOs.Identity;
using Microsoft.Extensions.Options;
using Tickets.Domain.Entity;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Tickets.Application.Command.User.Handlers
{
    public class LoginCommandHandler : IRequestHandler<LoginCommand, APIResponse<LoginResponseDto>>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly JwtSettings _jwtSettings;
        private readonly IAppLocalizer _localizer;

        public LoginCommandHandler(
            UserManager<ApplicationUser> userManager,
            IOptions<JwtSettings> jwtSettings,
            IAppLocalizer localizer)
        {
            _userManager = userManager;
            _jwtSettings = jwtSettings.Value;
            _localizer = localizer;
        }

        public async Task<APIResponse<LoginResponseDto>> Handle(LoginCommand request, CancellationToken cancellationToken)
        {
            if (request.LoginRequest == null)
                throw new ArgumentException(_localizer[LocalizationMessages.LoginPayloadRequired]);

            if (string.IsNullOrWhiteSpace(request.LoginRequest.Username) ||
                string.IsNullOrWhiteSpace(request.LoginRequest.Password))
                throw new ArgumentException(_localizer[LocalizationMessages.UsernameAndPasswordRequired]);

            var username = request.LoginRequest.Username;
            var user = await _userManager.FindByNameAsync(username)
                       ?? await _userManager.FindByEmailAsync(username)
                       ?? await _userManager.Users.FirstOrDefaultAsync(u => u.PhoneNumber == username, cancellationToken);

            if (user == null || user.IsDeleted || !user.IsActive)
                throw new ArgumentException(_localizer[LocalizationMessages.UserNotFound]);

            var isPasswordValid = await _userManager.CheckPasswordAsync(user, request.LoginRequest.Password);
            if (!isPasswordValid)
                throw new ArgumentException(_localizer[LocalizationMessages.InvalidPassword]);

            var roles = await _userManager.GetRolesAsync(user);
            var accessToken = TokenGenerator.GenerateAccessToken(user, roles, _jwtSettings);
            var refreshToken = TokenGenerator.GenerateRefreshToken();

            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryUTC = DateTime.UtcNow.AddDays(7);
            await _userManager.UpdateAsync(user);

            var response = new LoginResponseDto
            {
                UserId = user.Id,
                FullName = user.FullName,
                RefreshToken = user.RefreshToken,
                AccessToken = accessToken,
                Roles = roles.ToList(),
                FirstTimeLogin = user.FirstTimeLogin,
            };

            return APIResponse<LoginResponseDto>.Success(response, _localizer[LocalizationMessages.LoginSuccessful]);
        }
    }
}
