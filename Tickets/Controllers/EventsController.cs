using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tickets.Application.Command.Event;
using Tickets.Application.Command.EventOwner;
using Tickets.Application.Queries.Event;
using Tickets.Application.Queries.EventOwner;
using Tickets.Application.DTOs.Event;
using Tickets.Application.DTOs.EventOwner;
using Tickets.Domain.Enums;
using Tickets.Domain.Models;
using Tickets.Application.DTOs;
using Microsoft.AspNetCore.Identity;
using Tickets.Domain.Entity;
using Tickets.Application.Common.Localization;
using Tickets.Application.DTOs.Checkout;
using Tickets.Application.Query.Checkout;
using Tickets.Application.Common.Interfaces;

namespace Tickets.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    //[Authorize]
    public class EventsController : ApiController
    {
        private readonly IExcelService _excelService;

        public EventsController(ISender mediator, UserManager<ApplicationUser> userManager, IAppLocalizer localizer, IExcelService excelService) 
            : base(mediator, userManager, localizer)
        {
            _excelService = excelService;
        }

        [HttpGet("GetAllEvents/download-excel")]
        [AllowAnonymous]
        public async Task<IActionResult> DownloadAllEventsExcel([FromQuery] EventType? type)
        {
            var result = await _mediator.Send(new GetAllEventsQuery(type));
            var excelFile = _excelService.GenerateExcel(result, "Events");
            return File(excelFile, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"Events_{DateTime.Now:yyyyMMddHHmmss}.xlsx");
        }

        [HttpGet("{eventId}/attendees/download-excel")]
        public async Task<IActionResult> DownloadAttendeesExcel(Guid eventId)
        {
            var result = await _mediator.Send(new GetTicketsByEventIdQuery(eventId));
            var excelFile = _excelService.GenerateExcel(result.Data, "Attendees");
            return File(excelFile, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"Attendees_{eventId}_{DateTime.Now:yyyyMMddHHmmss}.xlsx");
        }

        [HttpGet("GetAllEventOwners/download-excel")]
        public async Task<IActionResult> DownloadAllEventOwnersExcel()
        {
            var result = await _mediator.Send(new GetAllEventOwnersQuery());
            var excelFile = _excelService.GenerateExcel(result, "EventOwners");
            return File(excelFile, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"EventOwners_{DateTime.Now:yyyyMMddHHmmss}.xlsx");
        }

        [HttpGet("GetEventsByOwner/download-excel/{userId}")]
        public async Task<IActionResult> DownloadEventsByOwnerExcel(string userId)
        {
            var result = await _mediator.Send(new GetEventsByOwnerQuery(userId));
            var excelFile = _excelService.GenerateExcel(result, "OwnedEvents");
            return File(excelFile, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"EventsByOwner_{userId}_{DateTime.Now:yyyyMMddHHmmss}.xlsx");
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

        [HttpGet("{eventId}/attendees")]
        public async Task<ActionResult<APIResponse<IEnumerable<AttendeeDto>>>> GetAttendees(Guid eventId)
        {
            var result = await _mediator.Send(new GetTicketsByEventIdQuery(eventId));
            return Ok(result);
        }

        #region Event Owner Endpoints

        [HttpPost("AssignEventToOwner")]
        public async Task<IActionResult> AssignEventToOwner([FromBody] AssignEventToOwnerRequest request)
        {
            var result = await _mediator.Send(new AssignEventToOwnerCommand(request.UserId, request.EventId));
            return Ok(result);
        }

        [HttpGet("GetAllEventOwners")]
        public async Task<IActionResult> GetAllEventOwners()
        {
            var result = await _mediator.Send(new GetAllEventOwnersQuery());
            return Ok(APIResponse<List<EventOwnerDto>>.Success(result, _localizer[LocalizationMessages.EventOwnersRetrievedSuccessfully]));
        }

        [HttpGet("GetEventsByOwner/{userId}")]
        public async Task<IActionResult> GetEventsByOwner(string userId)
        {
            var result = await _mediator.Send(new GetEventsByOwnerQuery(userId));
            return Ok(APIResponse<List<EventDto>>.Success(result, _localizer[LocalizationMessages.EventsByOwnerRetrievedSuccessfully]));
        }

        #endregion

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
