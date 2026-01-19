using Tickets.API.Resources;
using Tickets.Application.Command.User;
using Tickets.Application.Common.Localization;
using Tickets.Application.Queries.User;
using Tickets.Domain.Entity;
using Tickets.Application.DTOs;
using Tickets.Application.DTOs.Identity;
using Tickets.Application.DTOs.User;
using Tickets.Domain.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using System.ComponentModel.DataAnnotations;

namespace Tickets.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    [Produces("application/json")]
    public class UserManagerController : ApiController
    {
        #region Constructor
        public UserManagerController(
            ISender mediator, 
            UserManager<ApplicationUser> userManager,
            IAppLocalizer localizer)
            : base(mediator, userManager, localizer)
        {
        }
        #endregion

        #region User Registration
        [AllowAnonymous]
        [HttpPost("RegisterNewUser")]
        [ProducesResponseType(typeof(APIResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(APIResponse<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(APIResponse<string>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(APIResponse<string>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(APIResponse<string>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Register([FromBody] UserForRegisterDto model, CancellationToken ct)
        {
            if (model == null)
                throw new ValidationException(_localizer[LocalizationMessages.BodyRequired]);
            var performedBy = User?.Identity?.Name ?? "System";
            
            var result = await _mediator.Send(new RegisterCommand(model, performedBy), ct);
            return Ok(result);
        }
        #endregion

        #region User Update
        [HttpPut("Update")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(APIResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(APIResponse<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(APIResponse<string>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(APIResponse<string>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(APIResponse<string>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(APIResponse<string>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Update(string id, [FromBody] UserForUpdateDto model, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(id) || model == null || id != model.Id)
                throw new ArgumentException(_localizer[LocalizationMessages.UserDataMissingOrInconsistent]);
            var performedBy = User?.Identity?.Name ?? "System";
            var result = await _mediator.Send(new UpdateUserAsyncCommand(model, performedBy), ct);
            return Ok(APIResponse<bool>.Success(result, _localizer[LocalizationMessages.UserUpdatedSuccessfully]));
        }
        #endregion

        #region User Activation
        [HttpPost("SwitchUserActive/{id}")]
        [ProducesResponseType(typeof(APIResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(APIResponse<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(APIResponse<string>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(APIResponse<string>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(APIResponse<string>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> SwitchUserActive(Guid id, CancellationToken ct)
        {
            if (id == Guid.Empty)
                throw new ArgumentException(_localizer[LocalizationMessages.InvalidUserId]);
            var performedBy = User?.Identity?.Name ?? "System";
            var result = await _mediator.Send(new SwitchUserActiveCommand(id, performedBy), ct);
            return Ok(APIResponse<object>.Success(null, result 
                ? _localizer[LocalizationMessages.UserActivatedSuccessfully] 
                : _localizer[LocalizationMessages.UserDeactivated]));
        }
        #endregion

        #region User Queries
        [HttpGet("GetById/{id}")]
        [ProducesResponseType(typeof(APIResponse<UserForListDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(APIResponse<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(APIResponse<string>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(APIResponse<string>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(APIResponse<string>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetById(string id, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentException(_localizer[LocalizationMessages.InvalidUserId]);
            var userInfo = await _mediator.Send(new GetUserById(id), ct);
            return Ok(APIResponse<UserForListDto>.Success(userInfo, _localizer[LocalizationMessages.UserInfoRetrieved]));
        }

        [HttpGet("GetAll")]
        [ProducesResponseType(typeof(APIResponse<List<UserForListDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(APIResponse<string>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(APIResponse<string>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAll(CancellationToken ct)
        {
            var users = await _mediator.Send(new GetAllUser(), ct);
            return Ok(APIResponse<List<UserForListDto>>.Success(users, _localizer[LocalizationMessages.UsersInfoRetrieved]));
        }
        #endregion

        #region User Deletion
        [HttpDelete("DeleteUser/{userId}")]
        [ProducesResponseType(typeof(APIResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(APIResponse<string>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(APIResponse<string>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(APIResponse<string>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(APIResponse<string>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteUser(string userId, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException(_localizer[LocalizationMessages.InvalidUserId]);
            var performedBy = User?.Identity?.Name ?? "System";
            var result = await _mediator.Send(new DeleteUserCommand(userId, performedBy), ct);
            if (!result)
                throw new KeyNotFoundException(_localizer[LocalizationMessages.UserNotFoundOrDeleted]);
            return Ok(APIResponse<bool>.Success(true, _localizer[LocalizationMessages.UserDeletedSuccessfully]));
        }
        #endregion

        #region Role Management
        [HttpPut("UpdateUserRoles")]
        public async Task<IActionResult> UpdateUserRoles([FromBody] UpdateUserRolesRequest body, CancellationToken ct)
        {
            var performedBy = User?.Identity?.Name ?? "System";
            var result = await _mediator.Send(
                new UpdateUserRolesCommand(body.UserId, body.Roles, performedBy), ct);
            return Ok(APIResponse<UpdateUserRolesResult>
                .Success(result, _localizer[LocalizationMessages.UserRolesUpdated]));
        }
        #endregion
    }
}
