using MediatR;
using Tickets.Application.Common.Interfaces;
using Tickets.Application.Common.Localization;
using Tickets.Application.DTOs;
using Tickets.Application.DTOs.Checkout;
using Tickets.Domain.IRepository;

namespace Tickets.Application.Command.Checkout.Handlers
{
    public class ValidateQrCodeCommandHandler : IRequestHandler<ValidateQrCodeCommand, APIResponse<TicketDto>>
    {
        private readonly ITicketRepository _ticketRepository;
        private readonly IUnitOfWork _uow;
        private readonly IAppLocalizer _localizer;

        public ValidateQrCodeCommandHandler(
            ITicketRepository ticketRepository,
            IUnitOfWork uow,
            IAppLocalizer localizer)
        {
            _ticketRepository = ticketRepository;
            _uow = uow;
            _localizer = localizer;
        }

        public async Task<APIResponse<TicketDto>> Handle(ValidateQrCodeCommand request, CancellationToken cancellationToken)
        {
            var ticket = await _ticketRepository.GetByQrTokenAsync(request.Token, cancellationToken);

            if (ticket == null)
            {
                return APIResponse<TicketDto>.Fail(404, null, "Invalid QR Code.");
            }

            if (ticket.ScannedCount >= ticket.MaxScans)
            {
                return APIResponse<TicketDto>.Fail(400, null, "QR Code has reached its maximum scan limit.");
            }

            // Increment scan count
            ticket.ScannedCount++;
            await _ticketRepository.UpdateAsync(ticket);
            await _uow.CommitAsync();

            var dto = new TicketDto
            {
                Id = ticket.Id,
                AttendeeName = ticket.AttendeeName,
                AttendeeEmail = ticket.AttendeeEmail,
                AttendeePhone = ticket.AttendeePhone,
                AttendeeImageUrl = ticket.AttendeeImageUrl,
                VisitorCount = ticket.VisitorCount,
                MaxScans = ticket.MaxScans,
                ScannedCount = ticket.ScannedCount,
                EventName = ticket.Event?.Name ?? "N/A"
            };

            return APIResponse<TicketDto>.Success(dto, "QR Code validated successfully.");
        }
    }
}
