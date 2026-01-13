using System.Security.Claims;
namespace FleetLinker.Domain.IRepository
{
    public interface ITokenRepository
    {
        Task<ClaimsPrincipal>? GetPrincipalFromExpiredToken(string? token);
    }
}
