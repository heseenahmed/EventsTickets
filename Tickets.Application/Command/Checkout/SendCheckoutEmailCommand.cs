using MediatR;
using Tickets.Application.DTOs;

namespace Tickets.Application.Command.Checkout
{
    public record SendCheckoutEmailCommand(Guid TicketId) : IRequest<APIResponse<bool>>;
}
