namespace FleetLinker.Domain.Models
{
    public sealed class UpdateUserRolesResult
    {
        public string UserId { get; set; } = default!;
        public int AddedCount { get; set; }
        public int RemovedCount { get; set; }
        public int KeptCount { get; set; }
        public List<RoleSummaryDto> FinalRoles { get; set; } = new();
    }

    public sealed class RoleSummaryDto
    {
        public string Id { get; set; } = default!;
        public string Name { get; set; } = default!;
    }
}
