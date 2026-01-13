using System.ComponentModel.DataAnnotations;

namespace Tickets.Domain.Enums
{
    public enum Provider
    {
        [Display(Name = "Apple")]
        Apple,
        [Display(Name = "Web")]
        Web,
    }
}
