using MediatR;
using Microsoft.EntityFrameworkCore;
using Tickets.Application.DTOs;
using Tickets.Application.DTOs.Checkout;
using Tickets.Domain.IRepository;

namespace Tickets.Application.Query.Checkout.Handlers
{
    public class GetMyCheckoutsQueryHandler : IRequestHandler<GetMyCheckoutsQuery, APIResponse<List<MyCheckoutDto>>>
    {
        private readonly IBookingRepository _bookingRepository;

        public GetMyCheckoutsQueryHandler(IBookingRepository bookingRepository)
        {
            _bookingRepository = bookingRepository;
        }

        public async Task<APIResponse<List<MyCheckoutDto>>> Handle(GetMyCheckoutsQuery request, CancellationToken cancellationToken)
        {
            var bookings = await _bookingRepository.GetAllQueryable()
                .Where(b => b.StudentId == request.UserId)
                .Include(b => b.Event)
                .OrderByDescending(b => b.BookingDate)
                .Select(b => new MyCheckoutDto
                {
                    BookingId = b.Id,
                    EventId = b.EventId,
                    EventName = b.Event.Name,
                    TotalAmount = b.TotalPrice,
                    TotalVisitors = b.MaxEntries,
                    PaymentStatus = b.IsPaid ? "Paid" : "Pending",
                    CheckoutDate = b.BookingDate
                })
                .ToListAsync(cancellationToken);

            return APIResponse<List<MyCheckoutDto>>.Success(bookings, "Checkouts retrieved successfully.");
        }
    }
}
