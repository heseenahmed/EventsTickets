using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Tickets.Application.Common;
using Tickets.Application.Common.Interfaces;
using Tickets.Application.Common.Localization;
using Tickets.Application.DTOs;
using Tickets.Domain.Entity;
using Tickets.Domain.IRepository;

namespace Tickets.Application.Command.Checkout.Handlers
{
    public class SendCheckoutEmailCommandHandler : IRequestHandler<SendCheckoutEmailCommand, APIResponse<bool>>
    {
        private readonly ITicketRepository _ticketRepository;
        private readonly IEventRepository _eventRepository;
        private readonly IEmailSender _emailSender;
        private readonly MailSettings _mailSettings;
        private readonly IAppLocalizer _localizer;
        private readonly IUnitOfWork _uow;

        public SendCheckoutEmailCommandHandler(
            ITicketRepository ticketRepository,
            IEventRepository eventRepository,
            IEmailSender emailSender,
            IOptions<MailSettings> mailSettings,
            IAppLocalizer localizer,
            IUnitOfWork uow)
        {
            _ticketRepository = ticketRepository;
            _eventRepository = eventRepository;
            _emailSender = emailSender;
            _mailSettings = mailSettings.Value;
            _localizer = localizer;
            _uow = uow;
        }

        public async Task<APIResponse<bool>> Handle(SendCheckoutEmailCommand request, CancellationToken cancellationToken)
        {
            var initialTicket = await _ticketRepository.GetByGuidAsync(request.TicketId);
            if (initialTicket == null)
            {
                return APIResponse<bool>.Fail(404, null, _localizer[LocalizationMessages.NotFound]);
            }

            var eventEntity = await _eventRepository.GetByGuidAsync(initialTicket.EventId);
            if (eventEntity == null)
            {
                return APIResponse<bool>.Fail(404, null, _localizer[LocalizationMessages.NotFound]);
            }

            // Get all tickets related to this registration (same email and event)
            var tickets = await _ticketRepository.GetAllQueryable()
                .Where(t => t.EventId == initialTicket.EventId && t.AttendeeEmail == initialTicket.AttendeeEmail)
                .ToListAsync(cancellationToken);

            if (!tickets.Any())
            {
                return APIResponse<bool>.Fail(404, null, _localizer[LocalizationMessages.NotFound]);
            }

            // Generate QR tokens if missing
            bool updated = false;
            foreach (var ticket in tickets)
            {
                if (string.IsNullOrEmpty(ticket.QrToken))
                {
                    ticket.QrToken = Guid.NewGuid().ToString("N") + Guid.NewGuid().ToString("N");
                    await _ticketRepository.UpdateAsync(ticket);
                    updated = true;
                }
            }

            if (updated)
            {
                await _uow.CommitAsync();
            }

            try
            {
                var qrLinks = string.Join("<br/>", tickets.Select(t => $"<a href='https://tikcktat.vercel.app/qrcode/{t.QrToken}'>QR Code {tickets.IndexOf(t) + 1}</a>"));
                
                var subject = string.Format(_localizer[LocalizationMessages.EmailSubjectWelcome], eventEntity.Name);
                var message = string.Format(_localizer[LocalizationMessages.EmailBodyWelcomeTemplate], eventEntity.Name, initialTicket.AttendeeName, qrLinks, tickets.Count);

                await _emailSender.SendEmailAsync(
                    _mailSettings.Host,
                    _mailSettings.Port,
                    true,
                    _mailSettings.Email,
                    _mailSettings.Password,
                    initialTicket.AttendeeEmail,
                    subject,
                    message,
                    _mailSettings.DisplayName,
                    _mailSettings.Email);

                return APIResponse<bool>.Success(true, _localizer[LocalizationMessages.CheckoutSuccessfulWithEmail]);
            }
            catch (Exception ex)
            {
                return APIResponse<bool>.Fail(500, new List<string> { ex.Message }, "Email sending failed.");
            }
        }
    }
}
