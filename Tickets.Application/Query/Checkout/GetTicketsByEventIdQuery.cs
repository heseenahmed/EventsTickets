using MediatR;
using Tickets.Application.DTOs;
using Tickets.Application.DTOs.Checkout;

namespace Tickets.Application.Query.Checkout
{
    public class GetTicketsByEventIdQuery : IRequest<APIResponse<IEnumerable<AttendeeDto>>>
    {
        public Guid EventId { get; set; }

        public GetTicketsByEventIdQuery(Guid eventId)
        {
            EventId = eventId;
        }
    }
}
