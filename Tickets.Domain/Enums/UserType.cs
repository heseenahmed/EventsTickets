using System.ComponentModel.DataAnnotations;

namespace FleetLinker.Domain.Enums
{
    public enum UserType
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
