using Tickets.Application.Command.Core;
using Tickets.Domain.Entity;
using Tickets.Application.DTOs;
using Tickets.Application.DTOs.Identity;
using Tickets.Application.DTOs.User;
using Tickets.Domain.Models;
using MediatR;
using Microsoft.Extensions.Primitives;
using System.Security.Claims;
namespace Tickets.Application.Command.User
{
    public class UpdateUserAsyncCommand : IRequest<bool> 
    {
        public UserForUpdateDto UpdateUserDto { set; get; }
        public string? PerformedBy { get; }
        public UpdateUserAsyncCommand(UserForUpdateDto updateUserDto , string? performedBy)
        {
            UpdateUserDto = updateUserDto;   
            PerformedBy = performedBy;
        }
    }
    public sealed record GetPrincipalFromExpiredTokenCommand(string? Token) : IRequest<ClaimsPrincipal>;
    public class RegisterCommand : IRequest<bool>
    {
        public UserForRegisterDto userDto{set;get;}
        public string? PerformedBy{set;get;}
        public RegisterCommand(UserForRegisterDto userForRegisterDto , string? performedBy)
        {
            userDto = userForRegisterDto;
            PerformedBy = performedBy;
        }
    }
    public sealed record SwitchUserActiveCommand(Guid Id , string? PerformedBy) : IRequest<bool>;
    public class LoginCommand : IRequest<APIResponse<LoginResponseDto>>
    {
        public LoginRequest LoginRequest { get; set; }
        public LoginCommand(LoginRequest loginRequest)
        {
            LoginRequest = loginRequest;
        }
    }
    public class DeleteUserCommand : IRequest<bool> 
    {
        public string UserId { get; set; }
        public string? PerformedBy { get; set; }
        public DeleteUserCommand(string userId, string? performedBy)
        {
            UserId = userId;
            PerformedBy = performedBy;
        }
    }
    public sealed record UpdateUserRolesCommand(
      string UserId, List<Guid> RoleIds , string? PerformedBy) : IRequest<UpdateUserRolesResult>;
}
