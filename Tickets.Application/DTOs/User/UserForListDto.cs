using System.ComponentModel.DataAnnotations;
namespace FleetLinker.Application.DTOs.User
{
    public class UserForListDto
    {
        public string Id { get; set; }
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public string? Mobile { get; set; }
        public List<string>? UserRoles { get; set; }
        public bool IsActive { get; set; }
    }
}
