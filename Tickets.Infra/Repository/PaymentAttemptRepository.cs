using Tickets.Domain.Entity;
using Tickets.Domain.IRepository;
using Tickets.Infra.Data;

namespace Tickets.Infra.Repository
{
    public class PaymentAttemptRepository : BaseRepository<PaymentAttempt>, IPaymentAttemptRepository
    {
        public PaymentAttemptRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}
