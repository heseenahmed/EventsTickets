using MediatR;
using Microsoft.AspNetCore.Hosting;
using Tickets.Application.Common;
using Tickets.Application.Common.Interfaces;
using Tickets.Application.Common.Localization;
using Tickets.Application.DTOs;
using Tickets.Domain.Entity;
using Tickets.Domain.IRepository;
using Tickets.Application.Command.Checkout;
using Tickets.Domain.Enums;
using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore;


namespace Tickets.Application.Command.Checkout.Handlers
{
    public class CheckoutCommandHandler : IRequestHandler<CheckoutCommand, APIResponse<string>>
    {
        private readonly IEventRepository _eventRepository;
        private readonly ITicketRepository _ticketRepository;
        private readonly IUnitOfWork _uow;
        private readonly IWebHostEnvironment _env;
        private readonly IAppLocalizer _localizer;
        private readonly IEmailSender _emailSender;
        private readonly MailSettings _mailSettings;

        public CheckoutCommandHandler(
            IEventRepository eventRepository,
            ITicketRepository ticketRepository,
            IUnitOfWork uow,
            IWebHostEnvironment env,
            IAppLocalizer localizer,
            IEmailSender emailSender,
            IOptions<MailSettings> mailSettings)
        {
            _eventRepository = eventRepository;
            _ticketRepository = ticketRepository;
            _uow = uow;
            _env = env;
            _localizer = localizer;
            _emailSender = emailSender;
            _mailSettings = mailSettings.Value;
        }

        public async Task<APIResponse<string>> Handle(CheckoutCommand request, CancellationToken cancellationToken)
        {
            var eventEntity = await _eventRepository.GetByGuidAsync(request.Dto.EventId);
            if (eventEntity == null)
            {
                return APIResponse<string>.Fail(404, null, _localizer[LocalizationMessages.NotFound]);
            }

            // Rule: Each ticket has 2 default free visitors ADDED to the declared count (except for Fun Day events).
            // Scans = 1 (Attendee) + VisitorCount + 2 (Free Buffer).
            int totalPeople = eventEntity.Type == EventType.FunDayEvent 
                ? request.Dto.VisitorCount + 1 
                : request.Dto.VisitorCount + 3;

            if (eventEntity.NumberOfVisitorsAllowed > 0 && eventEntity.AvailableNumberOfVisitors < totalPeople)
            {
                return APIResponse<string>.Fail(400, null, _localizer[LocalizationMessages.NotEnoughTickets]);
            }

            // Prevent duplicate registration for the same event by email or phone
            bool alreadyRegistered = await _ticketRepository.GetAllQueryable()
                .AnyAsync(t => t.EventId == request.Dto.EventId && 
                               (eventEntity.Type == EventType.FunDayEvent 
                                ? t.AttendeePhone == request.Dto.Phone 
                                : (t.AttendeeEmail == request.Dto.Email || t.AttendeePhone == request.Dto.Phone)), cancellationToken);

            if (alreadyRegistered)
            {
                return APIResponse<string>.Fail(400, null, _localizer[LocalizationMessages.AlreadyRegistered]);
            }

            // Calculate Total Price
            // Base Price includes Attendee + 2 Free Visitors.
            // All input visitors are considered "Extra" and charged the fee.
            decimal totalPrice = eventEntity.Type == EventType.FunDayEvent 
                ? request.Dto.Price 
                : eventEntity.Price + (request.Dto.VisitorCount * eventEntity.VisitorFee);

            string? attendeeImageUrl = null;
            if (request.Dto.Photo != null)
            {
                var folderPath = Path.Combine(_env.WebRootPath, "uploads");
                attendeeImageUrl = await ImageHelper.SaveImageAsync(request.Dto.Photo, folderPath, request.BaseUrl);
            }

            var ticket = new Ticket
            {
                Id = Guid.NewGuid(),
                EventId = request.Dto.EventId,
                StudentId = request.StudentId,
                AttendeeName = request.Dto.FullName,
                AttendeeEmail = eventEntity.Type == EventType.FunDayEvent ? (request.Dto.Email ?? "benzenydev@gmail.com") : request.Dto.Email!,
                AttendeePhone = request.Dto.Phone,
                AttendeeImageUrl = attendeeImageUrl,
                VisitorCount = request.Dto.VisitorCount, // Keep original declared count
                TotalPrice = totalPrice,
                MaxScans = totalPeople, // 1 + VisitorCount + 2
                ScannedCount = 0,
                QrToken = Guid.NewGuid().ToString("N") + Guid.NewGuid().ToString("N"), // Longer token
                CreatedBy = request.Dto.FullName
            };

            if (eventEntity.NumberOfVisitorsAllowed > 0)
            {
                // Decrease available number of visitors by the reserved capacity (totalPeople)
                eventEntity.AvailableNumberOfVisitors -= totalPeople;
            }

            await _ticketRepository.AddAsync(ticket);
            await _eventRepository.UpdateAsync(eventEntity);
            await _uow.CommitAsync();

            try
            {
                var qrLink = $"https://tikcktat.vercel.app/qrcode/{ticket.QrToken}";
                var subject = string.Format(_localizer[LocalizationMessages.EmailSubjectWelcome], eventEntity.Name);
                var message = string.Format(_localizer[LocalizationMessages.EmailBodyWelcomeTemplate], eventEntity.Name, ticket.AttendeeName, qrLink);

                var recipientEmail = eventEntity.Type == EventType.FunDayEvent ? "benzenydev@gmail.com" : ticket.AttendeeEmail;

                await _emailSender.SendEmailAsync(
                    _mailSettings.Host,
                    _mailSettings.Port,
                    true,
                    _mailSettings.Email,
                    _mailSettings.Password,
                    recipientEmail,
                    subject,
                    message,
                    _mailSettings.DisplayName,
                    _mailSettings.Email);
            }
            catch (Exception ex)
            {
                // Log exception if needed, but don't fail the checkout if email fails
            }

            return APIResponse<string>.Success(ticket.QrToken, _localizer[LocalizationMessages.CheckoutSuccessfulWithEmail]);
        }
    }
}
