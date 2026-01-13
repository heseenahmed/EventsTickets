using System.Security.Claims;
namespace Tickets.Domain.IRepository
{
    public interface ITokenRepository
    {
        Task<ClaimsPrincipal>? GetPrincipalFromExpiredToken(string? token);
    }
}
