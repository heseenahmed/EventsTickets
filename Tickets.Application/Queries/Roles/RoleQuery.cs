using Tickets.Domain.Entity;
using Tickets.Application.DTOs.Identity;
using MediatR;
namespace Tickets.Application.Queries.Roles
{
    public record GetRoleList : IRequest<IEnumerable<ApplicationRole>>;
}
