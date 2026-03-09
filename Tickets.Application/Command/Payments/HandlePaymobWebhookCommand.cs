using MediatR;
using Tickets.Application.Common.Interfaces;

namespace Tickets.Application.Command.Payments
{
    public class HandlePaymobWebhookCommand : IRequest<bool>
    {
        public string Hmac { get; set; } = null!;
        public string RawBody { get; set; } = null!;
    }

    public class HandlePaymobWebhookCommandHandler : IRequestHandler<HandlePaymobWebhookCommand, bool>
    {
        private readonly IPaymentService _paymentService;

        public HandlePaymobWebhookCommandHandler(IPaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        public async Task<bool> Handle(HandlePaymobWebhookCommand request, CancellationToken cancellationToken)
        {
            return await _paymentService.ProcessCallbackAsync(request.Hmac, request.RawBody);
        }
    }
}
