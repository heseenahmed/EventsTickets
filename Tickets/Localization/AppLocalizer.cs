using FleetLinker.API.Resources;
using FleetLinker.Application.Common.Localization;
using Microsoft.Extensions.Localization;

namespace FleetLinker.API.Localization
{
    public class AppLocalizer : IAppLocalizer
    {
        private readonly IStringLocalizer<Messages> _localizer;

        public AppLocalizer(IStringLocalizer<Messages> localizer)
        {
            _localizer = localizer;
        }

        public string this[string key] => _localizer[key];
    }
}
