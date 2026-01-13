using FleetLinker.API.Resources;
using FleetLinker.Application.Command.Roles;
using FleetLinker.Application.Common.Localization;
using FleetLinker.Application.Queries.Roles;
using FleetLinker.Domain.Entity;
using FleetLinker.Application.DTOs;
using FleetLinker.Application.DTOs.Identity;
using FleetLinker.Domain.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using System.ComponentModel.DataAnnotations;

namespace FleetLinker.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    [Produces("application/json")]
    public class RolesController : ApiController
    {
        #region Constructor
        public RolesController(
            ISender mediator, 
            UserManager<ApplicationUser> userManager,
            IAppLocalizer localizer)
            : base(mediator, userManager, localizer) { }
        #endregion

        #region Get Roles
        [HttpGet("GetAllRoles")]
        [ProducesResponseType(typeof(APIResponse<List<ApplicationRole>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(APIResponse<string>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(APIResponse<string>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllRoles(CancellationToken ct)
        {
            var roles = await _mediator.Send(new GetRoleList(), ct);
            return Ok(APIResponse<List<ApplicationRole>>.Success(roles.ToList(), _localizer[LocalizationMessages.RolesRetrievedSuccessfully]));
        }
        #endregion

        #region Add Role
        [HttpPost("AddNewRole")]
        [ProducesResponseType(typeof(APIResponse<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(APIResponse<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(APIResponse<string>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(APIResponse<string>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(APIResponse<string>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> AddRole([FromBody] RoleDto role, CancellationToken ct)
        {
            if (role == null || string.IsNullOrWhiteSpace(role.Name))
                throw new ValidationException(_localizer[LocalizationMessages.RoleNameRequired]);
            var result = await _mediator.Send(new AddRoleCommand(role), ct);
            if (!result)
                throw new ApplicationException(_localizer[LocalizationMessages.ErrorAddingRole]);
            return Ok(APIResponse<string>.Success(_localizer[LocalizationMessages.RoleAddedSuccessfully]));
        }
        #endregion

        #region Delete Role
        [HttpDelete("DeleteRole/roleName")]
        [ProducesResponseType(typeof(APIResponse<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(APIResponse<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(APIResponse<string>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(APIResponse<string>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(APIResponse<string>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteByName(string roleName, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(roleName))
                throw new ArgumentException(_localizer[LocalizationMessages.RoleNameCannotBeNull]);
            var result = await _mediator.Send(new DeleteRoleCommand(roleName), ct);
            if (!result)
                throw new KeyNotFoundException(string.Format(_localizer[LocalizationMessages.RoleNotFound], roleName));
            return Ok(APIResponse<string>.Success(_localizer[LocalizationMessages.RoleDeletedSuccessfully]));
        }
        #endregion
    }
}
