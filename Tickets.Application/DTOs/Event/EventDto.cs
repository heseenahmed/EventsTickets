using System.ComponentModel.DataAnnotations;
using Tickets.Domain.Enums;
using Microsoft.AspNetCore.Http;
using Tickets.Application.Common.Mappings;
using Tickets.Domain.Entity;


namespace Tickets.Application.DTOs.Event
{
    public class EventDto : ImapFrom<Tickets.Domain.Entity.Event>
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public string Location { get; set; } = null!;
        public DateTime Date { get; set; }
        public decimal Price { get; set; }
        public decimal VisitorFee { get; set; }
        public int NumberOfVisitorsAllowed { get; set; }
        public int AvailableNumberOfVisitors { get; set; }
        public string? ImageUrl { get; set; }
        public string? EventDetails { get; set; }
        public string? TermsOfEntries { get; set; }
        public EventType Type { get; set; }

        // Owner Info
        public string? EventOwnerName { get; set; }
        public string? EventOwnerEmail { get; set; }
        public string? EventOwnerPhone { get; set; }

        // Status and Booking Info
        public string Status { get; set; } = null!;
        public int TotalVisitorsCount { get; set; }
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
        public decimal VisitorFee { get; set; }
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
        public decimal VisitorFee { get; set; }
        public int NumberOfVisitorsAllowed { get; set; }
        public int AvailableNumberOfVisitors { get; set; }
        public string? EventDetails { get; set; }
        public string? TermsOfEntries { get; set; }
        public EventType Type { get; set; }
        public IFormFile? Image { get; set; }
    }
}
