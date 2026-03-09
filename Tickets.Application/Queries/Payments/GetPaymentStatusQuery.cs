using MediatR;
using Tickets.Application.Common.Interfaces;
using Tickets.Application.DTOs;

namespace Tickets.Application.Queries.Payments
{
    public class GetPaymentStatusQuery : IRequest<APIResponse<PaymentStatusResponse>>
    {
        public Guid ReferenceId { get; set; }
        public string ReferenceType { get; set; } = "Ticket";
    }

    public class GetPaymentStatusQueryHandler : IRequestHandler<GetPaymentStatusQuery, APIResponse<PaymentStatusResponse>>
    {
        private readonly IPaymentService _paymentService;

        public GetPaymentStatusQueryHandler(IPaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        public async Task<APIResponse<PaymentStatusResponse>> Handle(GetPaymentStatusQuery request, CancellationToken cancellationToken)
        {
            var result = await _paymentService.GetPaymentStatusAsync(request.ReferenceId, request.ReferenceType);
            return APIResponse<PaymentStatusResponse>.Success(result);
        }
    }
}
