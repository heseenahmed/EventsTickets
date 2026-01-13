namespace FleetLinker.Application.DTOs.User
{
    public class UpdateUserRolesRequest
    {
        public string UserId { get; set; } = default!;
        public List<Guid> Roles { get; set; } = new();
    }
}
