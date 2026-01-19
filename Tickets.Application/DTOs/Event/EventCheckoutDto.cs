using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Tickets.Application.DTOs.Event
{
    public class EventCheckoutDto
    {
        [Required]
        public Guid EventId { get; set; }

        [Required]
        public string FullName { get; set; } = null!;

        [Required]
        public string Phone { get; set; } = null!;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = null!;

        public int VisitorCount { get; set; }

        public IFormFile? Photo { get; set; }
    }
}
