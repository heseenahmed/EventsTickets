using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Tickets.Application.Command.Booking;
using Tickets.Application.DTOs.Booking;
using Tickets.Application.DTOs;
using Tickets.Application.Common.Localization;
using Tickets.Domain.Entity;
using Tickets.Domain.Models;

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

        [HttpPost("scan")]
        public async Task<ActionResult<APIResponse<bool>>> ScanBooking([FromBody] ScanBookingCommand command)
        {
            return await _mediator.Send(command);
        }
    }
}
