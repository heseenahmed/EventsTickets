using System.ComponentModel.DataAnnotations;
using FleetLinker.Domain.Enums;
namespace FleetLinker.Application.DTOs.Identity
{
    public class TokenApiDto
    {
        public string? AccessToken { get; set; }
        public string? RefreshToken { get; set; }
        public string? Email { get; set; }
        public string? FullName { get; set; }
        public string? Mobile { get; set; }
        public bool User { get; set; }
        public bool HasPassword { get; set; }
        public Provider Provider { get; set; }
        public string? Id { get; set; }
    }
}
