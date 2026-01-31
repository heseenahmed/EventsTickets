using MediatR;
using Tickets.Application.DTOs.Event;
using Tickets.Application.DTOs.EventOwner;

namespace Tickets.Application.Queries.EventOwner
{
    public sealed record GetAllEventOwnersQuery() : IRequest<List<EventOwnerDto>>;
    public sealed record GetEventsByOwnerQuery(string UserId) : IRequest<List<EventDto>>;
}
