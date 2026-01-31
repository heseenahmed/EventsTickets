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

        [HttpPost("checkout")]
        [AllowAnonymous] // Assuming checkout can be done by non-logged in users as well, or as per requirement
        public async Task<ActionResult<APIResponse<string>>> Checkout([FromForm] CheckoutRequestDto dto)
        {
            var studentId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.PathBase}";
            
            var result = await _mediator.Send(new CheckoutCommand(dto, studentId, baseUrl));
            return Ok(result);
        }

        [HttpPost("validateQr/{token}")]
        [AllowAnonymous]
        public async Task<ActionResult<APIResponse<TicketDto>>> ValidateQr(string token)
        {
            var result = await _mediator.Send(new ValidateQrCodeCommand(token));
            return Ok(result);
        }
    }
}
