using MediatR;
using Microsoft.AspNetCore.Hosting;
using Tickets.Application.Common;
using Tickets.Application.Common.Interfaces;
using Tickets.Application.Common.Localization;
using Tickets.Application.DTOs;
using Tickets.Domain.Entity;
using Tickets.Domain.IRepository;

namespace Tickets.Application.Command.Event.Handlers
{
    public class EventCheckoutCommandHandler : IRequestHandler<EventCheckoutCommand, APIResponse<bool>>
    {
        private readonly IEventRepository _eventRepository;
        private readonly IBookingRepository _bookingRepository;
        private readonly IUnitOfWork _uow;
        private readonly IWebHostEnvironment _env;
        private readonly IAppLocalizer _localizer;

        public EventCheckoutCommandHandler(
            IEventRepository eventRepository,
            IBookingRepository bookingRepository,
            IUnitOfWork uow,
            IWebHostEnvironment env,
            IAppLocalizer localizer)
        {
            _eventRepository = eventRepository;
            _bookingRepository = bookingRepository;
            _uow = uow;
            _env = env;
            _localizer = localizer;
        }

        public async Task<APIResponse<bool>> Handle(EventCheckoutCommand request, CancellationToken cancellationToken)
        {
            var eventEntity = await _eventRepository.GetByGuidAsync(request.Dto.EventId);
            if (eventEntity == null)
            {
                return APIResponse<bool>.Fail(404, null, _localizer[LocalizationMessages.NotFound]);
            }

            int requiredVisitors = request.Dto.VisitorCount + 1; // Himself + visitors

            if (eventEntity.AvailableNumberOfVisitors < requiredVisitors)
            {
                return APIResponse<bool>.Fail(400, null, "Not enough available tickets.");
            }

            string? attendeeImageUrl = null;
            if (request.Dto.Photo != null)
            {
                var folderPath = Path.Combine(_env.WebRootPath, "uploads");
                attendeeImageUrl = await ImageHelper.SaveImageAsync(request.Dto.Photo, folderPath, request.BaseUrl);
            }

            var booking = new Tickets.Domain.Entity.Booking
            {
                Id = Guid.NewGuid(),
                StudentId = request.StudentId,
                EventId = request.Dto.EventId,
                AttendeeName = request.Dto.FullName,
                AttendeeEmail = request.Dto.Email,
                AttendeePhone = request.Dto.Phone,
                AttendeeImageUrl = attendeeImageUrl,
                NumberOfVisitors = request.Dto.VisitorCount,
                TotalPrice = eventEntity.Price * requiredVisitors,
                IsPaid = true,
                QrCodeData = Guid.NewGuid().ToString("N"),
                MaxEntries = requiredVisitors,
                CurrentEntries = 0,
                BookingDate = DateTime.UtcNow,
                CreatedBy = request.Dto.FullName
            };

            // Decrease available number of visitors
            eventEntity.AvailableNumberOfVisitors -= requiredVisitors;

            await _bookingRepository.AddAsync(booking);
            await _eventRepository.UpdateAsync(eventEntity);
            await _uow.CommitAsync();

            return APIResponse<bool>.Success(true, "Checkout successful.");
        }
    }
}
