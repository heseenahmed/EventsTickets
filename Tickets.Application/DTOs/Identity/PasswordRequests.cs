using System.ComponentModel.DataAnnotations;

namespace FleetLinker.Application.DTOs.Identity
{
    public class ChangePasswordApi
    {
        [Required]
        public string UserId { get; set; } = default!;
        [Required]
        public string OldPassword { get; set; } = default!;
        [Required]
        public string NewPassword { get; set; } = default!;
    }

    public class ForgetPasswordApi
    {
        [Required(ErrorMessage = "MobileNumberRequired")]
        [Display(Name = "Mobile")]
        [RegularExpression(@"^(\+\d{1,3}[- ]?)?\d{10}$", ErrorMessage = "MobileNumberRequired")]
        public string Mobile { get; set; } = default!;
        public string? Code { get; set; }
        public string? Password { get; set; }
    }
}
