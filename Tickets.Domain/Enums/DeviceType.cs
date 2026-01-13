using System.ComponentModel.DataAnnotations;

namespace FleetLinker.Domain.Enums
{
    public enum DeviceType
    {
        [Display(Name = "Apple iOS")]
        IOS = 1,
        [Display(Name = "Android")]
        Android,
        [Display(Name = "WEB")]
        WEB,
    }
}
