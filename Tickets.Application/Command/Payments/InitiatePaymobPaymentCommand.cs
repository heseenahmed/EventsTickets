using MediatR;
using Tickets.Application.Common.Interfaces;
using Tickets.Application.DTOs;

namespace Tickets.Application.Command.Payments
{
    public class InitiatePaymobPaymentCommand : IRequest<APIResponse<PaymentInitiationResult>>
    {
        public Guid ReferenceId { get; set; }
        public string ReferenceType { get; set; } = "Ticket";
        public string? UserId { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "EGP";
    }

    public class InitiatePaymobPaymentCommandHandler : IRequestHandler<InitiatePaymobPaymentCommand, APIResponse<PaymentInitiationResult>>
    {
        private readonly IPaymentService _paymentService;

        public InitiatePaymobPaymentCommandHandler(IPaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        public async Task<APIResponse<PaymentInitiationResult>> Handle(InitiatePaymobPaymentCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var result = await _paymentService.InitiatePaymentAsync(
                    request.ReferenceId, 
                    request.ReferenceType, 
                    request.Amount, 
                    request.Currency, 
                    request.UserId);

                return APIResponse<PaymentInitiationResult>.Success(result, "Payment initiated successfully.");
            }
            catch (Exception ex)
            {
                return APIResponse<PaymentInitiationResult>.Fail(400, new List<string> { ex.Message }, "Failed to initiate payment.");
            }
        }
    }
}
