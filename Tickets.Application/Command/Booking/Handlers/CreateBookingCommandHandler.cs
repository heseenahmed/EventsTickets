using AutoMapper;
using MediatR;
using Tickets.Application.Command.Booking;
using Tickets.Application.DTOs.Booking;
using Tickets.Application.Common.Interfaces;
using Tickets.Application.DTOs;
using Tickets.Domain.Entity;
using Tickets.Domain.IRepository;

namespace Tickets.Application.Command.Booking.Handlers
{
    public class CreateBookingCommandHandler : IRequestHandler<CreateBookingCommand, APIResponse<BookingDto>>
    {
        private readonly IBookingRepository _bookingRepository;
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;

        private const decimal BasePrice = 100m;
        private const decimal DependantPrice = 50m;

        public CreateBookingCommandHandler(
            IBookingRepository bookingRepository,
            IUserRepository userRepository,
            IMapper mapper,
            IUnitOfWork unitOfWork)
        {
            _bookingRepository = bookingRepository;
            _userRepository = userRepository;
            _mapper = mapper;
            _unitOfWork = unitOfWork;
        }

        public async Task<APIResponse<BookingDto>> Handle(CreateBookingCommand request, CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetByIdAsync(request.StudentId);
            if (user == null)
            {
                return APIResponse<BookingDto>.Fail(404, null, "Student not found.");
            }

            var existingBooking = await _bookingRepository.GetByStudentIdAsync(request.StudentId, cancellationToken);
            if (existingBooking != null)
            {
                return APIResponse<BookingDto>.Fail(400, null, "Student already has a booking.");
            }

            var booking = new Tickets.Domain.Entity.Booking
            {
                StudentId = request.StudentId,
                NumberOfVisitors = request.NumberOfVisitors,
                TotalPrice = BasePrice + (request.NumberOfVisitors * DependantPrice),
                IsPaid = true, // Assuming auto-pay for now or handled externally
                QrCodeData = Guid.NewGuid().ToString("N"),
                MaxEntries = 1 + request.NumberOfVisitors,
                CurrentEntries = 0,
                CreatedBy = user.FullName,
                AttendeeName = user.FullName,
                AttendeeEmail = user.Email ?? "",
                AttendeePhone = user.PhoneNumber ?? ""
            };

            await _bookingRepository.AddAsync(booking);
            await _unitOfWork.CommitAsync();

            var resultDto = _mapper.Map<BookingDto>(booking);
            resultDto.StudentName = user.FullName;

            return APIResponse<BookingDto>.Success(resultDto, "Booking created successfully.");
        }
    }
}
