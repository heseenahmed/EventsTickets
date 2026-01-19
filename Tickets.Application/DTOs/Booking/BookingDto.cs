using System;
using AutoMapper;
using Tickets.Application.Common.Mappings;
using Tickets.Domain.Entity;

namespace Tickets.Application.DTOs.Booking
{
    public class BookingDto : ImapFrom<Tickets.Domain.Entity.Booking>
    {
        public Guid Id { get; set; }
        public string StudentId { get; set; } = null!;
        public string StudentName { get; set; } = null!;
        public int NumberOfVisitors { get; set; }
        public Guid EventId { get; set; }
        public string AttendeeName { get; set; } = null!;
        public string AttendeePhone { get; set; } = null!;
        public string AttendeeEmail { get; set; } = null!;
        public string? AttendeeImageUrl { get; set; }
        public decimal TotalPrice { get; set; }
        public bool IsPaid { get; set; }
        public string QrCodeData { get; set; } = null!;
        public int MaxEntries { get; set; }
        public int CurrentEntries { get; set; }
        public DateTime BookingDate { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<Tickets.Domain.Entity.Booking, BookingDto>()
                .ForMember(d => d.StudentName, opt => opt.MapFrom(s => s.Student.FullName));
        }
    }
}
