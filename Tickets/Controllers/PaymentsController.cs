using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tickets.Application.Command.Payments;
using Tickets.Application.Queries.Payments;
using System.IO;
using System.Text;

namespace Tickets.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public PaymentsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("paymob/initiate")]
        [Authorize]
        public async Task<IActionResult> InitiatePaymob([FromBody] InitiatePaymobPaymentCommand command)
        {
            // Get UserId from claims if needed
            // command.UserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var response = await _mediator.Send(command);
            return StatusCode(response.ApiStatusCode, response);
        }

        [HttpGet("{referenceId}/status")]
        public async Task<IActionResult> GetStatus(Guid referenceId, [FromQuery] string referenceType = "Ticket")
        {
            var query = new GetPaymentStatusQuery { ReferenceId = referenceId, ReferenceType = referenceType };
            var response = await _mediator.Send(query);
            return StatusCode(response.ApiStatusCode, response);
        }

        [HttpPost("/api/webhooks/paymob")]
        [AllowAnonymous]
        public async Task<IActionResult> PaymobWebhook()
        {
            var hmac = Request.Headers["hmac"].ToString();
            
            using (var reader = new StreamReader(Request.Body, Encoding.UTF8))
            {
                var body = await reader.ReadToEndAsync();
                var command = new HandlePaymobWebhookCommand { Hmac = hmac, RawBody = body };
                var result = await _mediator.Send(command);
                
                // Always return 200 to Paymob to acknowledge receipt
                return Ok();
            }
        }
    }
}
