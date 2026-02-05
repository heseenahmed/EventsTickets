using MediatR;
using Tickets.Application.DTOs;
using Tickets.Application.DTOs.Checkout;
using Tickets.Domain.IRepository;

namespace Tickets.Application.Query.Checkout.Handlers
{
    public class GetTicketsByEventIdQueryHandler : IRequestHandler<GetTicketsByEventIdQuery, APIResponse<IEnumerable<AttendeeDto>>>
    {
        private readonly ITicketRepository _ticketRepository;

        public GetTicketsByEventIdQueryHandler(ITicketRepository ticketRepository)
        {
            _ticketRepository = ticketRepository;
        }

        public async Task<APIResponse<IEnumerable<AttendeeDto>>> Handle(GetTicketsByEventIdQuery request, CancellationToken cancellationToken)
        {
            var tickets = await _ticketRepository.GetByEventIdAsync(request.EventId, cancellationToken);
            
            var dtos = tickets.Select(t => new AttendeeDto
            {
                Id = t.Id,
                FullName = t.AttendeeName,
                PhotoUrl = t.AttendeeImageUrl,
                Phone = t.AttendeePhone,
                Email = t.AttendeeEmail,
                Companions = $"{t.VisitorCount} Companions",
                TotalPrice = $"{(t.Event.Price * (t.VisitorCount + 1)):N0} EGP",
                Status = t.ScannedCount < t.MaxScans ? "Active" : "Inactive"
            });

            return APIResponse<IEnumerable<AttendeeDto>>.Success(dtos);
        }
    }
}
