using FleetLinker.API.Resources;
using FleetLinker.Application.Common.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Localization;

namespace FleetLinker.API.Filter
{
    public class ValidateModelFilter : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (!context.ModelState.IsValid)
            {
                var localizer = context.HttpContext.RequestServices
                    .GetRequiredService<IStringLocalizer<Messages>>();
                
                var errors = context.ModelState
                    .Where(x => x.Value.Errors.Count > 0)
                    .ToDictionary(
                        error => error.Key,
                        error => error.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                    );
                context.Result = new BadRequestObjectResult(new
                {
                    Message = localizer[LocalizationMessages.ModelValidationFailed].Value,
                    Errors = errors
                });
            }
        }
    }
}