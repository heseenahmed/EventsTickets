namespace FleetLinker.Application.DTOs.Identity
{
    public class UserForUpdateDto
    {
        public string Id { get; set; }
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public string? Mobile { get; set; }
        public string? Username { get; set; }
        public string? Password { get; set; }
        public List<string>? RoleIds { get; set; }
    }
}
