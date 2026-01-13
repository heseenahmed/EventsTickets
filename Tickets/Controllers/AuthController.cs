using Tickets.API.Controllers;
using Tickets.API.Resources;
using Tickets.Application.Command.User;
using Tickets.Domain.Entity;
using Tickets.Application.DTOs;
using Tickets.Application.DTOs.Identity;
using Tickets.Domain.Models;
using Tickets.Application.Common;
using Tickets.Application.DTOs.Auth;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Tickets.Application.Common.Localization;

[Route("api/[controller]")]
[ApiController]
[Authorize]
[Produces("application/json")]
public class AuthController : ApiController
{
    #region Fields
    private readonly JwtSettings _jwtSettings;
    private readonly IDistributedCache _cache;

    #endregion

    #region Constructor
    public AuthController(
        ISender mediator,
        UserManager<ApplicationUser> userManager,
        IAppLocalizer localizer,
        IOptions<JwtSettings> jwtSettings,
        IDistributedCache cache) : base(mediator, userManager, localizer)
    {
        _jwtSettings = jwtSettings.Value;
        _cache = cache;
    }
    #endregion

    #region Login
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(APIResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(APIResponse<string>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(APIResponse<string>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(APIResponse<string>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken ct)
    {
        var result = await _mediator.Send(new LoginCommand(request), ct);
        return StatusCode(result.ApiStatusCode, result);
    }
    #endregion

    #region Logout
    [HttpPost("logout")]
    [ProducesResponseType(typeof(APIResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(APIResponse<string>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(APIResponse<string>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Logout()
    {
        bool logoutAll = true;
        var user = await _userManager.GetUserAsync(User)
                   ?? throw new UnauthorizedAccessException(_localizer[LocalizationMessages.UserNotAuthenticated]);
        user.RefreshToken = null;
        user.RefreshTokenExpiryUTC = null;
        if (logoutAll)
        {
            await _userManager.UpdateSecurityStampAsync(user);
        }
        else
        {
            var jti = User.FindFirst(JwtRegisteredClaimNames.Jti)?.Value;
            var expStr = User.FindFirst(JwtRegisteredClaimNames.Exp)?.Value;
            if (!string.IsNullOrWhiteSpace(jti) && long.TryParse(expStr, out var expUnix))
            {
                var exp = DateTimeOffset.FromUnixTimeSeconds(expUnix);
                var ttl = exp - DateTimeOffset.UtcNow;
                if (ttl < TimeSpan.Zero) ttl = TimeSpan.FromSeconds(5);
                var key = $"blk:{jti}";
                await _cache.SetStringAsync(
                    key,
                    "1",
                    new DistributedCacheEntryOptions
                    {
                        AbsoluteExpirationRelativeToNow = ttl
                    });
            }
        }
        var update = await _userManager.UpdateAsync(user);
        if (!update.Succeeded)
            throw new InvalidOperationException(string.Join("; ", update.Errors.Select(e => e.Description)));
        return Ok(APIResponse<object>.Success(null, logoutAll
            ? _localizer[LocalizationMessages.LogoutSuccessfulAllDevices]
            : _localizer[LocalizationMessages.LogoutSuccessful]));
    }
    #endregion

    #region Change Password
    [HttpPost("change-password")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(APIResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(APIResponse<string>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(APIResponse<string>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(APIResponse<string>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(APIResponse<string>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordApi change, CancellationToken ct)
    {
        if (change == null ||
        string.IsNullOrWhiteSpace(change.OldPassword) ||
        string.IsNullOrWhiteSpace(change.NewPassword))
        {
            throw new ArgumentException(_localizer[LocalizationMessages.OldAndNewPasswordsRequired]);
        }
        var user = await _userManager.Users.FirstOrDefaultAsync(e => e.Id == change.UserId, ct)
            ?? throw new KeyNotFoundException(_localizer[LocalizationMessages.UserNotFound]);
        var isPasswordValid = await _userManager.CheckPasswordAsync(user, change.OldPassword);
        if (!isPasswordValid)
            throw new UnauthorizedAccessException(_localizer[LocalizationMessages.InvalidCredentials]);
        var result = await _userManager.ChangePasswordAsync(
            user,
            change.OldPassword,
            change.NewPassword);
        if (!result.Succeeded)
            throw new InvalidOperationException(
                string.Join("; ", result.Errors.Select(e => e.Description)));
        // ?? VERY IMPORTANT
        await _userManager.UpdateSecurityStampAsync(user);
        user.FirstTimeLogin = false;
        await _userManager.UpdateAsync(user);
        var roles = await _userManager.GetRolesAsync(user);
        // ? Generate token AFTER password change
        var accessToken = TokenGenerator.GenerateAccessToken(
            user,
            roles,
            _jwtSettings);
        var response = new ChangePasswordResponseDto{AccessToken = accessToken};
        return Ok(APIResponse<object>.Success(response, _localizer[LocalizationMessages.PasswordChangedSuccessfully]));
    }
    #endregion

    #region Add New Password
    [HttpPost("AddNewPassword")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(APIResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(APIResponse<string>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(APIResponse<string>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(APIResponse<string>), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(APIResponse<string>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> AddNewPassword([FromBody] ForgetPasswordApi forget, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(forget?.Mobile) || string.IsNullOrWhiteSpace(forget.Password))
            throw new ArgumentException(_localizer[LocalizationMessages.MobileAndPasswordRequired]);
        var user = await _userManager.Users
            .FirstOrDefaultAsync(e => e.PhoneNumber == forget.Mobile && e.IsActive, ct)
            ?? throw new ApplicationException(_localizer[LocalizationMessages.UserNotFound]);
        user.PasswordHash = _userManager.PasswordHasher.HashPassword(user, forget.Password);
        await _userManager.UpdateAsync(user);
        await _userManager.UpdateSecurityStampAsync(user);
        return Ok(APIResponse<object>.Success(null, _localizer[LocalizationMessages.PasswordChangedSuccessfully]));
    }
    #endregion

    #region Refresh Token
    [HttpPost("refresh")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(APIResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(APIResponse<string>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(APIResponse<string>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(APIResponse<string>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> RefreshToken([FromBody] TokenRequest request, CancellationToken ct)
    {
        var principal = await _mediator.Send(new GetPrincipalFromExpiredTokenCommand(request.AccessToken), ct)
                        ?? throw new UnauthorizedAccessException(_localizer[LocalizationMessages.InvalidAccessToken]);
        var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value
                     ?? throw new UnauthorizedAccessException(_localizer[LocalizationMessages.InvalidAccessToken]);
        var user = await _userManager.FindByIdAsync(userId)
                   ?? throw new KeyNotFoundException(_localizer[LocalizationMessages.UserNotFound]);
        if (user.RefreshToken != request.RefreshToken || user.RefreshTokenExpiryUTC <= DateTime.UtcNow)
            throw new UnauthorizedAccessException(_localizer[LocalizationMessages.InvalidOrExpiredRefreshToken]);
        if(!user.IsActive || user.IsDeleted)
            throw new KeyNotFoundException(_localizer[LocalizationMessages.UserNotFoundOrInactive]);
        var newAccessToken = TokenGenerator.GenerateAccessToken(user, await _userManager.GetRolesAsync(user), _jwtSettings);
        user.RefreshToken = TokenGenerator.GenerateRefreshToken();
        user.RefreshTokenExpiryUTC = DateTime.UtcNow.AddDays(7);
        await _userManager.UpdateAsync(user);
        return Ok(APIResponse<object>.Success(new
        {
            AccessToken = newAccessToken,
            RefreshToken = user.RefreshToken,
            RefreshTokenExpiry = user.RefreshTokenExpiryUTC
        }, _localizer[LocalizationMessages.TokenRefreshedSuccessfully]));
    }
    #endregion
}
