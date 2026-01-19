using MediatR;
using Tickets.Domain.IRepository;
using Tickets.Application.DTOs.Event;
using Microsoft.EntityFrameworkCore;

namespace Tickets.Application.Queries.Event.Handlers
{
    public class EventQueryHandler : 
        IRequestHandler<GetEventByIdQuery, EventDto?>,
        IRequestHandler<GetAllEventsQuery, List<EventDto>>
    {
        private readonly IEventRepository _eventRepository;

        public EventQueryHandler(IEventRepository eventRepository)
        {
            _eventRepository = eventRepository;
        }

        public async Task<EventDto?> Handle(GetEventByIdQuery request, CancellationToken cancellationToken)
        {
            var eventEntity = await _eventRepository.GetByGuidAsync(request.Id);
            if (eventEntity == null) return null;

            return new EventDto
            {
                Id = eventEntity.Id,
                Name = eventEntity.Name,
                Description = eventEntity.Description,
                Location = eventEntity.Location,
                Date = eventEntity.Date,
                Price = eventEntity.Price,
                NumberOfVisitorsAllowed = eventEntity.NumberOfVisitorsAllowed,
                AvailableNumberOfVisitors = eventEntity.AvailableNumberOfVisitors,
                ImageUrl = eventEntity.ImageUrl,
                EventDetails = eventEntity.EventDetails,
                TermsOfEntries = eventEntity.TermsOfEntries,
                Type = eventEntity.Type
            };
        }

        public async Task<List<EventDto>> Handle(GetAllEventsQuery request, CancellationToken cancellationToken)
        {
            var query = _eventRepository.GetAllQueryable();
            
            if (request.Type.HasValue)
            {
                query = query.Where(e => e.Type == request.Type.Value);
            }

            return await query.Select(eventEntity => new EventDto
            {
                Id = eventEntity.Id,
                Name = eventEntity.Name,
                Description = eventEntity.Description,
                Location = eventEntity.Location,
                Date = eventEntity.Date,
                Price = eventEntity.Price,
                NumberOfVisitorsAllowed = eventEntity.NumberOfVisitorsAllowed,
                AvailableNumberOfVisitors = eventEntity.AvailableNumberOfVisitors,
                ImageUrl = eventEntity.ImageUrl,
                EventDetails = eventEntity.EventDetails,
                TermsOfEntries = eventEntity.TermsOfEntries,
                Type = eventEntity.Type
            }).ToListAsync(cancellationToken);
        }
    }
}
