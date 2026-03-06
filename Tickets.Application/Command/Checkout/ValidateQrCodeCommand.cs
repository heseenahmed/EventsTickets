using MediatR;
using Tickets.Application.DTOs;
using Tickets.Application.DTOs.Checkout;

namespace Tickets.Application.Command.Checkout
{
    public record ValidateQrCodeCommand(string Token) : IRequest<APIResponse<TicketDto>>;
}
