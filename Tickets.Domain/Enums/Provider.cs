using System.ComponentModel.DataAnnotations;

namespace FleetLinker.Domain.Enums
{
    public enum Provider
    {
        [Display(Name = "Apple")]
        Apple,
        [Display(Name = "Web")]
        Web,
    }
}
