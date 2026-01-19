using MediatR;
using Tickets.Application.DTOs.Booking;
using Tickets.Application.DTOs;

namespace Tickets.Application.Command.Booking
{
    public class CreateBookingCommand : IRequest<APIResponse<BookingDto>>
    {
        public string StudentId { get; set; } = null!;
        public int NumberOfVisitors { get; set; }
    }
}
