using MediatR;
using Tickets.Application.Common.Interfaces;
using Tickets.Domain.Entity;
using Tickets.Domain.IRepository;
using Tickets.Application.DTOs.Event;
using Tickets.Application.Common;
using Microsoft.AspNetCore.Hosting;

namespace Tickets.Application.Command.Event.Handlers
{
    public class EventCommandHandler : 
        IRequestHandler<CreateEventCommand, bool>,
        IRequestHandler<UpdateEventCommand, bool>,
        IRequestHandler<DeleteEventCommand, bool>
    {
        private readonly IEventRepository _eventRepository;
        private readonly IBookingRepository _bookingRepository;
        private readonly ITicketRepository _ticketRepository;
        private readonly IUnitOfWork _uow;
        private readonly IWebHostEnvironment _env;

        public EventCommandHandler(
            IEventRepository eventRepository, 
            IBookingRepository bookingRepository,
            ITicketRepository ticketRepository,
            IUnitOfWork uow, 
            IWebHostEnvironment env)
        {
            _eventRepository = eventRepository;
            _bookingRepository = bookingRepository;
            _ticketRepository = ticketRepository;
            _uow = uow;
            _env = env;
        }

        public async Task<bool> Handle(CreateEventCommand request, CancellationToken cancellationToken)
        {
            string? imageUrl = null;
            if (request.EventDto.Image != null)
            {
                var folderPath = Path.Combine(_env.WebRootPath, "uploads");
                imageUrl = await ImageHelper.SaveImageAsync(request.EventDto.Image, folderPath, request.BaseUrl);
            }

            var eventEntity = new Tickets.Domain.Entity.Event
            {
                Id = Guid.NewGuid(), // Automatic ID
                Name = request.EventDto.Name,
                Description = request.EventDto.Description,
                Location = request.EventDto.Location,
                Date = request.EventDto.Date,
                Price = request.EventDto.Price,
                VisitorFee = request.EventDto.VisitorFee,
                NumberOfVisitorsAllowed = request.EventDto.NumberOfVisitorsAllowed,
                AvailableNumberOfVisitors = request.EventDto.NumberOfVisitorsAllowed,
                ImageUrl = imageUrl,
                EventDetails = request.EventDto.EventDetails,
                TermsOfEntries = request.EventDto.TermsOfEntries,
                Type = request.EventDto.Type,
                CreatedBy = request.PerformedBy ?? "System",
                IsActive = true
            };

            await _eventRepository.AddAsync(eventEntity);
            return true;
        }

        public async Task<bool> Handle(UpdateEventCommand request, CancellationToken cancellationToken)
        {
            var eventEntity = await _eventRepository.GetByGuidAsync(request.EventDto.Id);
            if (eventEntity == null) return false;

            if (request.EventDto.Image != null)
            {
                // Delete old image if exists
                if (!string.IsNullOrEmpty(eventEntity.ImageUrl))
                {
                    ImageHelper.DeleteImage(eventEntity.ImageUrl);
                }

                var folderPath = Path.Combine(_env.WebRootPath, "uploads");
                eventEntity.ImageUrl = await ImageHelper.SaveImageAsync(request.EventDto.Image, folderPath, request.BaseUrl);
            }

            eventEntity.Name = request.EventDto.Name;
            eventEntity.Description = request.EventDto.Description;
            eventEntity.Location = request.EventDto.Location;
            eventEntity.Date = request.EventDto.Date;
            eventEntity.Price = request.EventDto.Price;
            eventEntity.VisitorFee = request.EventDto.VisitorFee;
            eventEntity.NumberOfVisitorsAllowed = request.EventDto.NumberOfVisitorsAllowed;
            eventEntity.AvailableNumberOfVisitors = request.EventDto.AvailableNumberOfVisitors;
            eventEntity.EventDetails = request.EventDto.EventDetails;
            eventEntity.TermsOfEntries = request.EventDto.TermsOfEntries;
            eventEntity.Type = request.EventDto.Type;
            eventEntity.UpdatedBy = request.PerformedBy;
            eventEntity.UpdatedDate = DateTime.UtcNow;

            await _eventRepository.UpdateAsync(eventEntity);
            return true;
        }

        public async Task<bool> Handle(DeleteEventCommand request, CancellationToken cancellationToken)
        {
            var eventEntity = await _eventRepository.GetByGuidAsync(request.Id);
            if (eventEntity == null) return false;

            // Cascading deletion: Delete associated bookings and tickets
            var bookings = await _bookingRepository.GetListAsync(b => b.EventId == request.Id);
            foreach (var booking in bookings)
            {
                await _bookingRepository.RemoveAsync(booking);
            }

            var tickets = await _ticketRepository.GetByEventIdAsync(request.Id, cancellationToken);
            foreach (var ticket in tickets)
            {
                await _ticketRepository.RemoveAsync(ticket);
            }

            await _eventRepository.RemoveAsync(eventEntity);
            await _uow.CommitAsync();

            return true;
        }
    }
}
