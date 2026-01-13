using Tickets.Application.Command.Core;
using Tickets.Domain.Models;
using MediatR;

namespace Tickets.Application.Command.Roles
{
    public abstract class RoleCommand : Commands
    {
        public RoleDto? Role { get; set; }
    }

    public class AddRoleCommand : RoleCommand, IRequest<bool>
    {
        public AddRoleCommand(RoleDto role)
        {
            Role = role;
        }
    }

    public class DeleteRoleCommand : IRequest<bool>
    {
        public string RoleName { get; set; }
        public DeleteRoleCommand(string roleName)
        {
            RoleName = roleName;
        }
    }

    public class UpdateRoleCommand : RoleCommand, IRequest<bool>
    {
        public UpdateRoleCommand(RoleDto role)
        {
            Role = role;
        }
    }
}
