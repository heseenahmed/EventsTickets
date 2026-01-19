using MediatR;
using Tickets.Application.DTOs.Event;
using Tickets.Domain.Enums;

namespace Tickets.Application.Queries.Event
{
    public record GetEventByIdQuery(Guid Id) : IRequest<EventDto?>;
    public record GetAllEventsQuery(EventType? Type) : IRequest<List<EventDto>>;
}
