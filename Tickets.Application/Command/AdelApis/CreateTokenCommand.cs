
using Tickets.Domain.Entity;
using MediatR;

namespace Tickets.Application.Command.AdelApis
{
    public class CreateTokenCommand : IRequest<List<Tokens>>
    {
        public string Token { get; set; } = string.Empty;
    }
}
