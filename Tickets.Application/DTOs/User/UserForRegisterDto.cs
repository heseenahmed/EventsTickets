using System.ComponentModel.DataAnnotations;
namespace Tickets.Application.DTOs.Identity
{
    public class UserForRegisterDto
    {
        public string FullName { get; set; } = null!;
        [EmailAddress]
        public string Email { get; set; } = null!;
        [Phone]
        public string Mobile { get; set; } = null!;
        public string Password { get; set; } = null!;
    }
}
