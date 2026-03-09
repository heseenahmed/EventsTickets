using MediatR;
using Tickets.Application.DTOs;
using Tickets.Application.DTOs.Checkout;

namespace Tickets.Application.Query.Checkout
{
    public record GetMyCheckoutsQuery(string UserId) : IRequest<APIResponse<List<MyCheckoutDto>>>;
}
