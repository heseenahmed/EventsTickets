using FleetLinker.Application.DTOs.Identity;
using FleetLinker.Application.DTOs.User;
using FleetLinker.Domain.Models;
using MediatR;

namespace FleetLinker.Application.Queries.User
{
    public sealed record GetUserInfoAsyncCommand(string Id) : IRequest<UserInfoAPI>;
    public sealed record GetUserById(string Id) : IRequest<UserForListDto>;
    public sealed record GetAllUser() : IRequest<List<UserForListDto>>;
}