using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Tickets.Application.DTOs.Checkout
{
    public class CheckoutRequestDto
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

    public class TicketDto
    {
        public Guid Id { get; set; }
        public string AttendeeName { get; set; } = null!;
        public string AttendeeEmail { get; set; } = null!;
        public string AttendeePhone { get; set; } = null!;
        public string? AttendeeImageUrl { get; set; }
        public int VisitorCount { get; set; }
        public int MaxScans { get; set; }
        public int ScannedCount { get; set; }
        public int RemainingScans => MaxScans - ScannedCount;
        public decimal TotalPrice { get; set; }
        public string EventName { get; set; } = null!;
    }
}
