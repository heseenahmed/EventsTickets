using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Tickets.Application.Command.Booking;
using Tickets.Application.Command.Event;
using Tickets.Application.DTOs.Booking;
using Tickets.Application.DTOs.Event;
using Tickets.Application.DTOs;
using Tickets.Application.Common.Localization;
using Tickets.Domain.Entity;
using Tickets.Domain.Models;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace Tickets.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BookingsController : ApiController
    {
        public BookingsController(ISender mediator, UserManager<ApplicationUser> userManager, IAppLocalizer localizer) 
            : base(mediator, userManager, localizer)
        {
        }

        [HttpPost("create")]
        public async Task<ActionResult<APIResponse<BookingDto>>> CreateBooking([FromBody] CreateBookingCommand command)
        {
            return await _mediator.Send(command);
        }

        [HttpPost("event-checkout")]
        [Authorize]
        public async Task<ActionResult<APIResponse<EventCheckoutResponseDto>>> EventCheckout([FromForm] EventCheckoutDto dto)
        {
            var studentId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.PathBase}";
            
            var result = await _mediator.Send(new EventCheckoutCommand(dto, studentId, baseUrl));
            return StatusCode(result.ApiStatusCode, result);
        }

        [HttpPost("scan")]
        public async Task<ActionResult<APIResponse<bool>>> ScanBooking([FromBody] ScanBookingCommand command)
        {
            return await _mediator.Send(command);
        }
    }
}
