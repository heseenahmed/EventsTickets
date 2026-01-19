using System.ComponentModel.DataAnnotations;
using Tickets.Domain.Enums;
using Microsoft.AspNetCore.Http;

namespace Tickets.Application.DTOs.Event
{
    public class EventDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public string Location { get; set; } = null!;
        public DateTime Date { get; set; }
        public decimal Price { get; set; }
        public int NumberOfVisitorsAllowed { get; set; }
        public int AvailableNumberOfVisitors { get; set; }
        public string? ImageUrl { get; set; }
        public string? EventDetails { get; set; }
        public string? TermsOfEntries { get; set; }
        public EventType Type { get; set; }
    }

    public class CreateEventDto
    {
        [Required]
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        [Required]
        public string Location { get; set; } = null!;
        public DateTime Date { get; set; }
        public decimal Price { get; set; }
        public int NumberOfVisitorsAllowed { get; set; }
        public string? EventDetails { get; set; }
        public string? TermsOfEntries { get; set; }
        public EventType Type { get; set; }
        public IFormFile? Image { get; set; }
    }

    public class UpdateEventDto
    {
        [Required]
        public Guid Id { get; set; }
        [Required]
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        [Required]
        public string Location { get; set; } = null!;
        public DateTime Date { get; set; }
        public decimal Price { get; set; }
        public int NumberOfVisitorsAllowed { get; set; }
        public int AvailableNumberOfVisitors { get; set; }
        public string? EventDetails { get; set; }
        public string? TermsOfEntries { get; set; }
        public EventType Type { get; set; }
        public IFormFile? Image { get; set; }
    }
}
