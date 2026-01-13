using System.ComponentModel.DataAnnotations;

namespace Tickets.Domain.Enums
{
    public enum Roles
    {
        [Display(Name = "Admin")]
        Admin = 1,
        [Display(Name = "Client")]
        Client,
        [Display(Name = "Visitor")]
        Visitor,
        [Display(Name = "Workshop")]
        Workshop
    }
}
