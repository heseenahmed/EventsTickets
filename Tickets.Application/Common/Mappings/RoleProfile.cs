using AutoMapper;
using Tickets.Domain.Entity;
using Tickets.Application.DTOs.Identity;
using Tickets.Domain.Models;
namespace Tickets.Application.Common.Mappings
{
    public class RoleProfile :Profile
    {
        public RoleProfile()
        {
            CreateMap<RoleDto, ApplicationRole>().ReverseMap();
        }
    }
}
