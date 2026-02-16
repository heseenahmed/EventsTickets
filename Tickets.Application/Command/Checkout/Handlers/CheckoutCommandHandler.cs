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
using Tickets.Application.Common.Options;


namespace Tickets.Application.Command.Checkout.Handlers
{
    public class CheckoutCommandHandler : IRequestHandler<CheckoutCommand, APIResponse<List<string>>>
    {
        private readonly IEventRepository _eventRepository;
        private readonly ITicketRepository _ticketRepository;
        private readonly IUnitOfWork _uow;
        private readonly IWebHostEnvironment _env;
        private readonly IAppLocalizer _localizer;
        private readonly IEmailSender _emailSender;
        private readonly MailSettings _mailSettings;
        private readonly IPaymobService _paymobService;
        private readonly PaymobOptions _paymobOptions;

        public CheckoutCommandHandler(
            IEventRepository eventRepository,
            ITicketRepository ticketRepository,
            IUnitOfWork uow,
            IWebHostEnvironment env,
            IAppLocalizer localizer,
            IEmailSender emailSender,
            IOptions<MailSettings> mailSettings,
            IPaymobService paymobService,
            IOptions<PaymobOptions> paymobOptions)
        {
            _eventRepository = eventRepository;
            _ticketRepository = ticketRepository;
            _uow = uow;
            _env = env;
            _localizer = localizer;
            _emailSender = emailSender;
            _mailSettings = mailSettings.Value;
            _paymobService = paymobService;
            _paymobOptions = paymobOptions.Value;
        }

        public async Task<APIResponse<List<string>>> Handle(CheckoutCommand request, CancellationToken cancellationToken)
        {
            var eventEntity = await _eventRepository.GetByGuidAsync(request.Dto.EventId);
            if (eventEntity == null)
            {
                return APIResponse<List<string>>.Fail(404, null, _localizer[LocalizationMessages.NotFound]);
            }

            // Rule: Each person has their own unique QR code.
            // Scans = 1 per person.
            int totalPeople = request.Dto.VisitorCount + 1; 

            if (eventEntity.NumberOfVisitorsAllowed > 0 && eventEntity.AvailableNumberOfVisitors < totalPeople)
            {
                return APIResponse<List<string>>.Fail(400, null, _localizer[LocalizationMessages.NotEnoughTickets]);
            }

            // Prevent duplicate registration for the same event by email or phone
            bool alreadyRegistered = await _ticketRepository.GetAllQueryable()
                .AnyAsync(t => t.EventId == request.Dto.EventId && 
                               (t.AttendeeEmail == request.Dto.Email || t.AttendeePhone == request.Dto.Phone), cancellationToken);

            if (alreadyRegistered)
            {
                return APIResponse<List<string>>.Fail(400, null, _localizer[LocalizationMessages.AlreadyRegistered]);
            }

            // Calculate Total Price
            decimal totalPrice = (eventEntity.Type == EventType.FunDayEvent || request.Dto.Price > 0)
                ? request.Dto.Price 
                : eventEntity.Price + (request.Dto.VisitorCount * eventEntity.VisitorFee);

            string? attendeeImageUrl = null;
            if (request.Dto.Photo != null)
            {
                var folderPath = Path.Combine(_env.WebRootPath, "uploads");
                attendeeImageUrl = await ImageHelper.SaveImageAsync(request.Dto.Photo, folderPath, request.BaseUrl);
            }

            var tickets = new List<Ticket>();
            for (int i = 0; i < totalPeople; i++)
            {
                var ticket = new Ticket
                {
                    Id = Guid.NewGuid(),
                    EventId = request.Dto.EventId,
                    StudentId = request.StudentId,
                    AttendeeName = i == 0 ? request.Dto.FullName : $"{request.Dto.FullName} - Visitor {i}",
                    AttendeeEmail = request.Dto.Email,
                    AttendeePhone = request.Dto.Phone,
                    AttendeeImageUrl = attendeeImageUrl,
                    VisitorCount = 0, // Individual ticket
                    TotalPrice = i == 0 ? totalPrice : 0, // Assign price to first ticket only
                    MaxScans = 1,
                    ScannedCount = 0,
                    QrToken = Guid.NewGuid().ToString("N") + Guid.NewGuid().ToString("N"),
                    IsPaid = true,
                    CreatedBy = request.Dto.FullName
                };
                tickets.Add(ticket);
                await _ticketRepository.AddAsync(ticket);
            }

            if (eventEntity.NumberOfVisitorsAllowed > 0)
            {
                eventEntity.AvailableNumberOfVisitors -= totalPeople;
            }

            await _eventRepository.UpdateAsync(eventEntity);

            string? paymentUrl = null;
            if (totalPrice > 0)
            {
                try
                {
                    var authToken = await _paymobService.GetAuthenticationTokenAsync();
                    var paymobOrderId = await _paymobService.CreateOrderAsync(authToken, totalPrice * 100); 
                    var paymentKey = await _paymobService.GeneratePaymentKeyAsync(authToken, paymobOrderId, totalPrice * 100);

                    paymentUrl = $"https://accept.paymob.com/api/acceptance/iframes/{_paymobOptions.IframeId}?payment_token={paymentKey}";
                    foreach(var t in tickets) t.IsPaid = false; 
                }
                catch (Exception ex)
                {
                    return APIResponse<List<string>>.Fail(500, new List<string> { ex.Message }, "Failed to initiate payment process.");
                }
            }

            await _uow.CommitAsync();

            try
            {
                var qrLinks = string.Join("<br/>", tickets.Select(t => $"<a href='https://tikcktat.vercel.app/qrcode/{t.QrToken}'>QR Code {tickets.IndexOf(t) + 1}</a>"));
                
                var subject = string.Format(_localizer[LocalizationMessages.EmailSubjectWelcome], eventEntity.Name);
                var message = string.Format(_localizer[LocalizationMessages.EmailBodyWelcomeTemplate], eventEntity.Name, request.Dto.FullName, qrLinks, tickets.Count);

                var recipientEmail = request.Dto.Email;

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
                // Log exception if needed
            }

            var response = APIResponse<List<string>>.Success(tickets.Select(t => t.QrToken).ToList(), _localizer[LocalizationMessages.CheckoutSuccessfulWithEmail]);
            response.PaymentUrl = paymentUrl;
            return response;
        }
    }
}
