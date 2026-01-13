using Tickets.Application.Command.Note;
using Tickets.Application.DTOs;
using Tickets.Domain.Entity;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Tickets.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NotesController : ControllerBase
    {
        private readonly IMediator _mediator;

        public NotesController(IMediator mediator)
        {
            _mediator = mediator;
        }
        // Body: { "title": "t", "details": "d" }
        [HttpPost("CreateNote")]
        public async Task<ActionResult<APIResponse<Note>>> Create([FromBody] CreateNoteCommand command, CancellationToken ct)
        {
            var result = await _mediator.Send(command, ct);

            var hasErrors = result.ApiStatusCode >= StatusCodes.Status400BadRequest
                || string.Equals(result.Result, "Error", StringComparison.OrdinalIgnoreCase)
                || (result.Errors?.Any() == true);

            if (hasErrors)
                return StatusCode(result.ApiStatusCode, result);

            return Ok(result);

        }
    }
}
