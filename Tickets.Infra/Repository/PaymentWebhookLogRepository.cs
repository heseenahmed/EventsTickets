using Tickets.Domain.Entity;
using Tickets.Domain.IRepository;
using Tickets.Infra.Data;

namespace Tickets.Infra.Repository
{
    public class PaymentWebhookLogRepository : BaseRepository<PaymentWebhookLog>, IPaymentWebhookLogRepository
    {
        public PaymentWebhookLogRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}
