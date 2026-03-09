using MediatR;
using Microsoft.AspNetCore.Hosting;
using Tickets.Application.Common;
using Tickets.Application.Common.Interfaces;
using Tickets.Application.Common.Localization;
using Tickets.Application.DTOs;
using Tickets.Application.DTOs.Event;
using Tickets.Domain.Entity;
using Tickets.Domain.IRepository;
using Tickets.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Tickets.Application.Command.Event.Handlers
{
    public class EventCheckoutCommandHandler : IRequestHandler<EventCheckoutCommand, APIResponse<EventCheckoutResponseDto>>
    {
        private readonly IEventRepository _eventRepository;
        private readonly IBookingRepository _bookingRepository;
        private readonly IPaymentService _paymentService;
        private readonly IUnitOfWork _uow;
        private readonly IWebHostEnvironment _env;
        private readonly IAppLocalizer _localizer;
        private readonly IMailRepository _mailRepository;
        private readonly ILogger<EventCheckoutCommandHandler> _logger;

        public EventCheckoutCommandHandler(
            IEventRepository eventRepository,
            IBookingRepository bookingRepository,
            IPaymentService paymentService,
            IUnitOfWork uow,
            IWebHostEnvironment env,
            IAppLocalizer localizer,
            IMailRepository mailRepository,
            ILogger<EventCheckoutCommandHandler> logger)
        {
            _eventRepository = eventRepository;
            _bookingRepository = bookingRepository;
            _paymentService = paymentService;
            _uow = uow;
            _env = env;
            _localizer = localizer;
            _mailRepository = mailRepository;
            _logger = logger;
        }

        public async Task<APIResponse<EventCheckoutResponseDto>> Handle(EventCheckoutCommand request, CancellationToken cancellationToken)
        {
            var dto = request.Dto;

            // 1. Validate Event exists
            var eventEntity = await _eventRepository.GetByGuidAsync(dto.EventId);
            if (eventEntity == null)
            {
                return APIResponse<EventCheckoutResponseDto>.Fail(404, null, _localizer[LocalizationMessages.NotFound]);
            }

            // 2. Check if user already booked this event (by email or phone)
            var existingBooking = await _bookingRepository.GetAllQueryable()
                .FirstOrDefaultAsync(b => b.EventId == dto.EventId &&
                    (eventEntity.Type == EventType.FunDayEvent
                        ? b.AttendeePhone == dto.Phone
                        : (b.AttendeeEmail == dto.Email || b.AttendeePhone == dto.Phone)), cancellationToken);

            if (existingBooking != null)
            {
                // Already booked AND paid
                if (existingBooking.IsPaid)
                {
                    return APIResponse<EventCheckoutResponseDto>.Fail(400, null, 
                        "You have already booked and paid for this event.");
                }

                // Already booked but NOT paid — return existing checkout URL
                try
                {
                    var paymentResult = await _paymentService.InitiatePaymentAsync(
                        existingBooking.Id,
                        "Booking",
                        existingBooking.TotalPrice,
                        "EGP",
                        request.StudentId,
                        dto.FullName,
                        dto.Email ?? "NA",
                        dto.Phone);

                    return APIResponse<EventCheckoutResponseDto>.Success(new EventCheckoutResponseDto
                    {
                        BookingId = existingBooking.Id,
                        CheckoutUrl = paymentResult.CheckoutUrl,
                        IsNewBooking = false,
                        Message = "You already booked this event but haven't paid yet. Please complete your payment."
                    }, "You already booked this event but haven't paid yet. Please complete your payment.");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to get payment URL for existing booking {BookingId}", existingBooking.Id);
                    return APIResponse<EventCheckoutResponseDto>.Fail(500, new List<string> { ex.Message },
                        "Failed to retrieve payment link. Please try again.");
                }
            }

            // 3. Validate available tickets
            int requiredVisitors = dto.VisitorCount + 1; // Himself + visitors
            if (eventEntity.AvailableNumberOfVisitors < requiredVisitors)
            {
                return APIResponse<EventCheckoutResponseDto>.Fail(400, null, "Not enough available tickets for this event.");
            }

            // 4. Create new booking
            string? attendeeImageUrl = null;
            if (dto.Photo != null)
            {
                var folderPath = Path.Combine(_env.WebRootPath, "uploads");
                attendeeImageUrl = await ImageHelper.SaveImageAsync(dto.Photo, folderPath, request.BaseUrl);
            }

            var booking = new Tickets.Domain.Entity.Booking
            {
                Id = Guid.NewGuid(),
                StudentId = request.StudentId,
                EventId = dto.EventId,
                AttendeeName = dto.FullName,
                AttendeeEmail = eventEntity.Type == EventType.FunDayEvent ? (dto.Email ?? "benzenydev@gmail.com") : dto.Email!,
                AttendeePhone = dto.Phone,
                AttendeeImageUrl = attendeeImageUrl,
                NumberOfVisitors = dto.VisitorCount,
                TotalPrice = eventEntity.Type == EventType.FunDayEvent ? dto.Price : eventEntity.Price * requiredVisitors,
                IsPaid = false,
                QrCodeData = Guid.NewGuid().ToString("N"),
                MaxEntries = requiredVisitors,
                CurrentEntries = 0,
                BookingDate = DateTime.UtcNow,
                CreatedBy = dto.FullName
            };

            // Decrease available number of visitors
            eventEntity.AvailableNumberOfVisitors -= requiredVisitors;

            await _bookingRepository.AddAsync(booking);
            await _eventRepository.UpdateAsync(eventEntity);
            await _uow.CommitAsync();

            // 5. Send email for FunDay events
            if (eventEntity.Type == EventType.FunDayEvent)
            {
                try
                {
                    var body = $"<h1>Fun Day Event Checkout</h1>" +
                               $"<p>Attendee: {booking.AttendeeName}</p>" +
                               $"<p>Phone: {booking.AttendeePhone}</p>" +
                               $"<p>Visitors: {booking.NumberOfVisitors}</p>" +
                               $"<p>Price: {booking.TotalPrice}</p>" +
                               $"<p>QR Code: {booking.QrCodeData}</p>";
                    await _mailRepository.SendEmailAsync("benzenydev@gmail.com", "Fun Day Event Checkout", body);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to send checkout email for booking {BookingId}", booking.Id);
                }
            }

            // 6. Initiate Paymob Payment
            try
            {
                var paymentResult = await _paymentService.InitiatePaymentAsync(
                    booking.Id,
                    "Booking",
                    booking.TotalPrice,
                    "EGP",
                    request.StudentId,
                    dto.FullName,
                    dto.Email ?? "NA",
                    dto.Phone);

                return APIResponse<EventCheckoutResponseDto>.Success(new EventCheckoutResponseDto
                {
                    BookingId = booking.Id,
                    CheckoutUrl = paymentResult.CheckoutUrl,
                    IsNewBooking = true,
                    Message = "Booking created successfully. Please complete your payment."
                }, "Booking created successfully. Please complete your payment.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Booking created but payment initiation failed for {BookingId}", booking.Id);
                return APIResponse<EventCheckoutResponseDto>.Fail(500, new List<string> { ex.Message },
                    "Booking was created but payment initiation failed. Please try again.");
            }
        }
    }
}
