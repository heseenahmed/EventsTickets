using MediatR;
using Tickets.Application.DTOs;

namespace Tickets.Application.Command.EventOwner
{
    public sealed record AssignEventToOwnerCommand(string UserId, Guid EventId) : IRequest<APIResponse<bool>>;
}
