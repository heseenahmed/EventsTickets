using Tickets.Application.DTOs.Identity;
using Tickets.Application.DTOs.User;
using Tickets.Domain.Models;
using MediatR;

namespace Tickets.Application.Queries.User
{
    public sealed record GetUserInfoAsyncCommand(string Id) : IRequest<UserInfoAPI>;
    public sealed record GetUserById(string Id) : IRequest<UserForListDto>;
    public sealed record GetAllUser() : IRequest<List<UserForListDto>>;
}
