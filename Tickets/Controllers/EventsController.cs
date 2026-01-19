using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tickets.Application.Command.Event;
using Tickets.Application.Queries.Event;
using Tickets.Application.DTOs.Event;
using Tickets.Domain.Enums;
using Tickets.Domain.Models;
using Tickets.Application.DTOs;
using Microsoft.AspNetCore.Identity;
using Tickets.Domain.Entity;
using Tickets.Application.Common.Localization;

namespace Tickets.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    //[Authorize]
    public class EventsController : ApiController
    {
        public EventsController(ISender mediator, UserManager<ApplicationUser> userManager, IAppLocalizer localizer) 
            : base(mediator, userManager, localizer)
        {
        }

        [HttpPost("CreateNewEvent")]
        public async Task<IActionResult> Create([FromForm] CreateEventDto model)
        {
            var performedBy = User?.Identity?.Name ?? "System";
            var baseUrl = $"{Request.Scheme}://{Request.Host}";
            var result = await _mediator.Send(new CreateEventCommand(model, performedBy, baseUrl));
            return Ok(APIResponse<bool>.Success(result, _localizer[LocalizationMessages.EventCreatedSuccessfully]));
        }

        [HttpPut("UpdateEvent")]
        public async Task<IActionResult> Update([FromForm] UpdateEventDto model)
        {
            var performedBy = User?.Identity?.Name ?? "System";
            var baseUrl = $"{Request.Scheme}://{Request.Host}";
            var result = await _mediator.Send(new UpdateEventCommand(model, performedBy, baseUrl));
            return Ok(APIResponse<bool>.Success(result, _localizer[LocalizationMessages.EventUpdatedSuccessfully]));
        }

        [HttpDelete("DeleteEvent/{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var performedBy = User?.Identity?.Name ?? "System";
            var result = await _mediator.Send(new DeleteEventCommand(id, performedBy));
            return Ok(APIResponse<bool>.Success(result, _localizer[LocalizationMessages.EventDeletedSuccessfully]));
        }

        [HttpGet("GetEventById/{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _mediator.Send(new GetEventByIdQuery(id));
            return Ok(APIResponse<EventDto?>.Success(result, _localizer[LocalizationMessages.EventRetrievedSuccessfully]));
        }

        [HttpGet("GetAllEvents")]
        [AllowAnonymous]
        public async Task<IActionResult> GetAll([FromQuery] EventType? type)
        {
            var result = await _mediator.Send(new GetAllEventsQuery(type));
            return Ok(APIResponse<List<EventDto>>.Success(result, _localizer[LocalizationMessages.EventsRetrievedSuccessfully]));
        }

        [HttpPost("Checkout")]
        [AllowAnonymous]
        public async Task<IActionResult> Checkout([FromForm] EventCheckoutDto model)
        {
            var baseUrl = $"{Request.Scheme}://{Request.Host}";
            var studentId = User?.Identity?.IsAuthenticated == true ? loggedInUserId : null;
            var result = await _mediator.Send(new EventCheckoutCommand(model, studentId, baseUrl));
            return Ok(result);
        }

        //[HttpGet("GetGraduationEvents")]
        //public async Task<IActionResult> GetGraduationEvents()
        //{
        //    var result = await _mediator.Send(new GetAllEventsQuery(EventType.GraduationParty));
        //    return Ok(APIResponse<List<EventDto>>.Success(result));
        //}

        //[HttpGet("GetConferenceEvents")]
        //public async Task<IActionResult> GetConferenceEvents()
        //{
        //    var result = await _mediator.Send(new GetAllEventsQuery(EventType.Conferences));
        //    return Ok(APIResponse<List<EventDto>>.Success(result));
        //}
    }
}
