using FleetLinker.Application.Command.AdelApis;
using FleetLinker.Domain.Entity;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FleetLinker.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdelTokensController : ControllerBase
    {
        private readonly IMediator _mediator;

        public AdelTokensController(IMediator mediator)
        {
            _mediator = mediator;
        }
        [HttpPost("CreateToken")]
        public async Task<ActionResult<List<Tokens>>> Create([FromBody] CreateTokenCommand command, CancellationToken ct)
        {
            var allTokens = await _mediator.Send(command, ct);
            return Ok(allTokens);
        }
    }
}
