namespace Tickets.Application.DTOs
{
    public class LoginResponseDto
    {
        public string UserId { get; set; } = null!;
        public string FullName { get; set; } = null!;
        public string? RefreshToken { get; set; }
        public string AccessToken { get; set; } = null!;
        public bool FirstTimeLogin { get; set; }
        public List<string> Roles { get; set; } = new();
    }
}
