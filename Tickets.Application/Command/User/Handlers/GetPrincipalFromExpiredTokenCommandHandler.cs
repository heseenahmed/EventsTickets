using Tickets.Application.Common.Localization;
using Tickets.Domain.IRepository;
using MediatR;
using System.Security.Claims;

namespace Tickets.Application.Command.User.Handlers
{
    public class GetPrincipalFromExpiredTokenCommandHandler : IRequestHandler<GetPrincipalFromExpiredTokenCommand, ClaimsPrincipal>
    {
        private readonly ITokenRepository _tokenRepository;
        private readonly IAppLocalizer _localizer;

        public GetPrincipalFromExpiredTokenCommandHandler(
            ITokenRepository tokenRepository,
            IAppLocalizer localizer)
        {
            _tokenRepository = tokenRepository;
            _localizer = localizer;
        }

        public async Task<ClaimsPrincipal> Handle(GetPrincipalFromExpiredTokenCommand request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.Token))
                throw new ArgumentException(_localizer[LocalizationMessages.TokenRequired]);

            var principal = await _tokenRepository.GetPrincipalFromExpiredToken(request.Token);
            if (principal == null)
                throw new UnauthorizedAccessException(_localizer[LocalizationMessages.InvalidOrExpiredToken]);

            return principal;
        }
    }
}
