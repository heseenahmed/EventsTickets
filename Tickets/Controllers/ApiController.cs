using FleetLinker.API.Resources;
using FleetLinker.Application.Common.Localization;
using FleetLinker.Domain.Entity;
using FleetLinker.Application.DTOs;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace FleetLinker.API.Controllers
{
    [ApiController]
    [Route("api/[controller]/v1/[action]")]
    [Authorize]
    public class ApiController : ControllerBase
    {
        #region Fields
        public readonly ISender _mediator;
        public readonly UserManager<ApplicationUser> _userManager;
        public readonly IAppLocalizer _localizer;
        public string loggedInUserId => _userManager.GetUserId(User) ?? "User";
        #endregion

        #region Constructor
        public ApiController(ISender mediator, UserManager<ApplicationUser> userManager, IAppLocalizer localizer)
        {
            _mediator = mediator;
            _userManager = userManager;
            _localizer = localizer;
        }
        #endregion

        #region Helper Methods
        protected IActionResult? ValidateGuidIdIfEmpty(Guid id)
        {
            if (id == Guid.Empty)
                return BadRequest(new APIResponse<string>
                {
                    Result = _localizer[LocalizationMessages.Failed],
                    Msg = _localizer[LocalizationMessages.InvalidId]
                });
            return null;
        }
        #endregion
    }
}