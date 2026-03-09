using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tickets.Application.Command.Checkout;
using Tickets.Application.DTOs;
using Tickets.Application.DTOs.Checkout;
using System.Security.Claims;
using Tickets.API.Controllers;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Tickets.Domain.Entity;
using Tickets.Application.Common.Localization;
using Tickets.Application.Query.Checkout;
using Microsoft.AspNetCore.Cors;

namespace Tickets.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CheckoutController : ApiController
    {
        public CheckoutController(ISender mediator, UserManager<ApplicationUser> userManager, IAppLocalizer localizer) 
            : base(mediator, userManager, localizer)
        {
        }

        [HttpGet("my-checkouts")]
        [Authorize]
        public async Task<ActionResult<APIResponse<List<MyCheckoutDto>>>> GetMyCheckouts()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var result = await _mediator.Send(new GetMyCheckoutsQuery(userId!));
            return StatusCode(result.ApiStatusCode, result);
        }

        [HttpPost("checkout")]
        [AllowAnonymous] 
        public async Task<ActionResult<APIResponse<Guid>>> Checkout([FromForm] CheckoutRequestDto dto)
        {
            var studentId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.PathBase}";
            
            var result = await _mediator.Send(new CheckoutCommand(dto, studentId, baseUrl));
            return StatusCode(result.ApiStatusCode, result);
        }

        [HttpPost("send-email/{ticketId}")]
        [AllowAnonymous]
        public async Task<ActionResult<APIResponse<bool>>> SendEmail(Guid ticketId)
        {
            var result = await _mediator.Send(new SendCheckoutEmailCommand(ticketId));
            return StatusCode(result.ApiStatusCode, result);
        }

        [HttpPost("validateQr/{token}")]
        [AllowAnonymous]
        [EnableCors("AllowAll")]
        public async Task<ActionResult<APIResponse<TicketDto>>> ValidateQr(string token)
        {
            var result = await _mediator.Send(new ValidateQrCodeCommand(token));
            return StatusCode(result.ApiStatusCode, result);
        }
    }
}
