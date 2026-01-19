using MediatR;
using Tickets.Application.DTOs;

namespace Tickets.Application.Command.Booking
{
    public class ScanBookingCommand : IRequest<APIResponse<bool>>
    {
        public string QrCodeData { get; set; } = null!;
    }
}
