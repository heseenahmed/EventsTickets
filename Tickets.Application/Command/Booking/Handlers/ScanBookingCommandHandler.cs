using MediatR;
using Tickets.Application.Common.Interfaces;
using Tickets.Domain.IRepository;
using Tickets.Application.DTOs;

namespace Tickets.Application.Command.Booking.Handlers
{
    public class ScanBookingCommandHandler : IRequestHandler<ScanBookingCommand, APIResponse<bool>>
    {
        private readonly IBookingRepository _bookingRepository;
        private readonly IUnitOfWork _unitOfWork;

        public ScanBookingCommandHandler(IBookingRepository bookingRepository, IUnitOfWork unitOfWork)
        {
            _bookingRepository = bookingRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<APIResponse<bool>> Handle(ScanBookingCommand request, CancellationToken cancellationToken)
        {
            var booking = await _bookingRepository.GetByQrCodeAsync(request.QrCodeData, cancellationToken);
            if (booking == null)
            {
                return APIResponse<bool>.Fail(404, null, "Booking not found.");
            }

            if (booking.CurrentEntries >= booking.MaxEntries)
            {
                return APIResponse<bool>.Fail(400, null, "All entries for this booking have been used.");
            }

            booking.CurrentEntries++;
            booking.UpdatedDate = DateTime.UtcNow;
            
            await _bookingRepository.UpdateAsync(booking);
            await _unitOfWork.CommitAsync();

            int remaining = booking.MaxEntries - booking.CurrentEntries;
            return APIResponse<bool>.Success(true, $"Entry permitted. Remaining entries: {remaining}");
        }
    }
}
