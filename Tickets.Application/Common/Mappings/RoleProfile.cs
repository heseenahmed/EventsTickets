using AutoMapper;
using FleetLinker.Domain.Entity;
using FleetLinker.Application.DTOs.Identity;
using FleetLinker.Domain.Models;
namespace FleetLinker.Application.Common.Mappings
{
    public class RoleProfile :Profile
    {
        public RoleProfile()
        {
            CreateMap<RoleDto, ApplicationRole>().ReverseMap();
        }
    }
}
