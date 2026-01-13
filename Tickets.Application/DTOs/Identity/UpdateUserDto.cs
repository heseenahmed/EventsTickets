using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
namespace Tickets.Application.DTOs.Identity
{
    public class UpdateUserDto
    {
        public string Id { get; set; }
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public string? Mobile { get; set; }
        public string? SSN { get; set; }
        public string? OldPassword { get; set; }
        public string? NewPassword { get; set; }
        public string? ConfirmNewPassword { get; set; }
        public string[]? Roles { get; set; }
    }
}
