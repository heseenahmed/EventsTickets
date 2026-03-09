using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tickets.Application.Command.Payments;
using Tickets.Application.Queries.Payments;
using System.IO;
using System.Text;
using Microsoft.Extensions.Logging;

namespace Tickets.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentsController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<PaymentsController> _logger;

        public PaymentsController(IMediator mediator, ILogger<PaymentsController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        [HttpPost("paymob/initiate")]
        [Authorize]
        public async Task<IActionResult> InitiatePaymob([FromBody] InitiatePaymobPaymentCommand command)
        {
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
            _logger.LogInformation("=== PAYMOB WEBHOOK RECEIVED ===");
            
            var hmac = Request.Headers["hmac"].ToString();
            _logger.LogInformation("HMAC from header: {Hmac}", string.IsNullOrEmpty(hmac) ? "EMPTY" : hmac);

            // Also check query string for HMAC (Paymob sometimes sends it there)
            if (string.IsNullOrEmpty(hmac) && Request.Query.ContainsKey("hmac"))
            {
                hmac = Request.Query["hmac"].ToString();
                _logger.LogInformation("HMAC from query: {Hmac}", hmac);
            }
            
            using (var reader = new StreamReader(Request.Body, Encoding.UTF8))
            {
                var body = await reader.ReadToEndAsync();
                _logger.LogInformation("Webhook body length: {Length}", body.Length);
                _logger.LogInformation("Webhook body (first 500 chars): {Body}", body.Length > 500 ? body[..500] : body);
                
                var command = new HandlePaymobWebhookCommand { Hmac = hmac, RawBody = body };
                var result = await _mediator.Send(command);
                
                _logger.LogInformation("Webhook processing result: {Result}", result);
                
                // Always return 200 to Paymob to acknowledge receipt
                return Ok();
            }
        }
    }
}
