
using FleetLinker.Domain.Entity;
using MediatR;

namespace FleetLinker.Application.Command.AdelApis
{
    public class CreateTokenCommand : IRequest<List<Tokens>>
    {
        public string Token { get; set; } = string.Empty;
    }
}
