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
            var eventEntity = await _eventRepository.GetAllQueryable()
                .Include(e => e.Owner)
                .Include(e => e.Tickets)
                .FirstOrDefaultAsync(e => e.Id == request.Id, cancellationToken);

            if (eventEntity == null) return null;

            return new EventDto
            {
                Id = eventEntity.Id,
                Name = eventEntity.Name,
                Description = eventEntity.Description,
                Location = eventEntity.Location,
                Date = eventEntity.Date,
                Price = eventEntity.Price,
                VisitorFee = eventEntity.VisitorFee,
                NumberOfVisitorsAllowed = eventEntity.NumberOfVisitorsAllowed,
                AvailableNumberOfVisitors = eventEntity.AvailableNumberOfVisitors,
                ImageUrl = eventEntity.ImageUrl,
                EventDetails = eventEntity.EventDetails,
                TermsOfEntries = eventEntity.TermsOfEntries,
                Type = eventEntity.Type,
                EventOwnerName = eventEntity.Owner?.FullName,
                EventOwnerEmail = eventEntity.Owner?.Email,
                EventOwnerPhone = eventEntity.Owner?.PhoneNumber,
                Status = eventEntity.Date > DateTime.UtcNow ? "Active" : "Inactive",
                TotalVisitorsCount = eventEntity.Tickets.Sum(t => t.VisitorCount)
            };
        }

        public async Task<List<EventDto>> Handle(GetAllEventsQuery request, CancellationToken cancellationToken)
        {
            IQueryable<Tickets.Domain.Entity.Event> query = _eventRepository.GetAllQueryable()
                .Include(e => e.Owner)
                .Include(e => e.Tickets);
            
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
                VisitorFee = eventEntity.VisitorFee,
                NumberOfVisitorsAllowed = eventEntity.NumberOfVisitorsAllowed,
                AvailableNumberOfVisitors = eventEntity.AvailableNumberOfVisitors,
                ImageUrl = eventEntity.ImageUrl,
                EventDetails = eventEntity.EventDetails,
                TermsOfEntries = eventEntity.TermsOfEntries,
                Type = eventEntity.Type,
                EventOwnerName = eventEntity.Owner != null ? eventEntity.Owner.FullName : null,
                EventOwnerEmail = eventEntity.Owner != null ? eventEntity.Owner.Email : null,
                EventOwnerPhone = eventEntity.Owner != null ? eventEntity.Owner.PhoneNumber : null,
                Status = eventEntity.Date > DateTime.UtcNow ? "Active" : "Inactive",
                TotalVisitorsCount = eventEntity.Tickets.Sum(t => t.VisitorCount)
            }).ToListAsync(cancellationToken);
        }
    }
}
