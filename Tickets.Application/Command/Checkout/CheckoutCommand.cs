using MediatR;
using Tickets.Application.DTOs;
using Tickets.Application.DTOs.Checkout;

namespace Tickets.Application.Command.Checkout
{
    public record CheckoutCommand(CheckoutRequestDto Dto, string? StudentId, string BaseUrl) : IRequest<APIResponse<string>>;
}
