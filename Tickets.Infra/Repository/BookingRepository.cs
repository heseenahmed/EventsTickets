using Microsoft.EntityFrameworkCore;
using Tickets.Domain.Entity;
using Tickets.Domain.IRepository;
using Tickets.Infra.Data;

namespace Tickets.Infra.Repository
{
    public class BookingRepository : BaseRepository<Booking>, IBookingRepository
    {
        private readonly ApplicationDbContext _context;

        public BookingRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<Booking?> GetByQrCodeAsync(string qrCodeData, CancellationToken ct)
        {
            return await _context.Bookings
                .Include(b => b.Student)
                .FirstOrDefaultAsync(b => b.QrCodeData == qrCodeData, ct);
        }

        public async Task<Booking?> GetByStudentIdAsync(string studentId, CancellationToken ct)
        {
            return await _context.Bookings
                .Include(b => b.Student)
                .FirstOrDefaultAsync(b => b.StudentId == studentId, ct);
        }
    }
}
