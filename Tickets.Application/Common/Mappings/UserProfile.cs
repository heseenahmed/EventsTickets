using AutoMapper;
using FleetLinker.Domain.Entity;
using FleetLinker.Application.DTOs.Identity;
using FleetLinker.Application.DTOs.User;
namespace FleetLinker.Application.Common.Mappings
{
    public class UserProfile : Profile
    {
        public UserProfile()
        {
            CreateMap<ApplicationUser, UserForListDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.FullName))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.Mobile, opt => opt.MapFrom(src => src.PhoneNumber))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive))
                .ForMember(dest => dest.UserRoles, opt => opt.MapFrom(src =>
                    src.UserRoles.Select(ur => ur.Role != null ? ur.Role.Name : null).Where(name => name != null).ToList()))
                .ReverseMap();
            CreateMap<ApplicationUser, GetUserDto>()
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Mobile, opt => opt.MapFrom(src => src.PhoneNumber));
        }
    }
}