using MediatR;
using Microsoft.AspNetCore.Hosting;
using Tickets.Application.Common;
using Tickets.Application.Common.Interfaces;
using Tickets.Application.Common.Localization;
using Tickets.Application.DTOs;
using Tickets.Domain.Entity;
using Tickets.Domain.IRepository;
using Tickets.Application.Command.Checkout;
using Microsoft.Extensions.Options;


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

            int requiredVisitors = request.Dto.VisitorCount + 1; // Himself + visitors

            if (eventEntity.AvailableNumberOfVisitors < requiredVisitors)
            {
                return APIResponse<string>.Fail(400, null, _localizer[LocalizationMessages.NotEnoughTickets]);
            }

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
                AttendeeEmail = request.Dto.Email,
                AttendeePhone = request.Dto.Phone,
                AttendeeImageUrl = attendeeImageUrl,
                VisitorCount = request.Dto.VisitorCount,
                MaxScans = requiredVisitors,
                ScannedCount = 0,
                QrToken = Guid.NewGuid().ToString("N") + Guid.NewGuid().ToString("N"), // Longer token
                CreatedBy = request.Dto.FullName
            };

            // Decrease available number of visitors
            eventEntity.AvailableNumberOfVisitors -= requiredVisitors;

            await _ticketRepository.AddAsync(ticket);
            await _eventRepository.UpdateAsync(eventEntity);
            await _uow.CommitAsync();

            try
            {
                var qrLink = $"https://tikcktat.vercel.app/qrcode/{ticket.QrToken}";
                var subject = string.Format(_localizer[LocalizationMessages.EmailSubjectWelcome], eventEntity.Name);
                var message = string.Format(_localizer[LocalizationMessages.EmailBodyWelcomeTemplate], eventEntity.Name, ticket.AttendeeName, qrLink);

                await _emailSender.SendEmailAsync(
                    _mailSettings.Host,
                    _mailSettings.Port,
                    true,
                    _mailSettings.Email,
                    _mailSettings.Password,
                    ticket.AttendeeEmail,
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
