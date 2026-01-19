using MediatR;
using Tickets.Application.DTOs.Event;
using Tickets.Application.DTOs;

namespace Tickets.Application.Command.Event
{
    public record EventCheckoutCommand(EventCheckoutDto Dto, string? StudentId, string BaseUrl) : IRequest<APIResponse<bool>>;
}
