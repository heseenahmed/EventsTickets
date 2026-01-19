using MediatR;
using Tickets.Application.DTOs.Event;

namespace Tickets.Application.Command.Event
{
    public record CreateEventCommand(CreateEventDto EventDto, string? PerformedBy, string BaseUrl) : IRequest<bool>;
    public record UpdateEventCommand(UpdateEventDto EventDto, string? PerformedBy, string BaseUrl) : IRequest<bool>;
    public record DeleteEventCommand(Guid Id, string? PerformedBy) : IRequest<bool>;
}
