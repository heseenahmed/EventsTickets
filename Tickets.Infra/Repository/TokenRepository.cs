using Tickets.Application.Common.Localization;
using Tickets.Domain.Entity;
using Tickets.Domain.IRepository;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
namespace Tickets.Infra.Repository
{
    public class TokenRepository : ITokenRepository
    {
        private readonly JwtSettings _jwtSettings;
        private readonly IAppLocalizer _localizer;
        public TokenRepository(IOptions<JwtSettings> jwtSettings , IAppLocalizer localizer)
        {
            _jwtSettings = jwtSettings.Value;
            _localizer = localizer;
        }
        public Task<ClaimsPrincipal> GetPrincipalFromExpiredToken(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
                throw new ArgumentException(_localizer[LocalizationMessages.TokenRequired]);
            var validationParams = new TokenValidationParameters
            {
                ValidateAudience = true,
                ValidateIssuer = true,
                ValidAudience = _jwtSettings.Audience,
                ValidIssuer = _jwtSettings.Issuer,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Secret)),
                ValidateLifetime = false,          // allow expired tokens (we're just extracting principal)
                ClockSkew = TimeSpan.Zero
            };
            var handler = new JwtSecurityTokenHandler();
            try
            {
                var principal = handler.ValidateToken(token, validationParams, out var securityToken);
                if (securityToken is not JwtSecurityToken jwt ||
                    !jwt.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                {
                    throw new UnauthorizedAccessException(_localizer[LocalizationMessages.InvalidTokenAlgorithm]);
                }
                return Task.FromResult(principal);
            }
            catch (SecurityTokenException ex)
            {
                throw new UnauthorizedAccessException(_localizer[LocalizationMessages.InvalidAccessToken], ex);
            }
            catch (Exception ex)
            {
                throw new UnauthorizedAccessException(_localizer[LocalizationMessages.InvalidAccessToken], ex);
            }
        }
    }
}
