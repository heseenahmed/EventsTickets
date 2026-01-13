using FleetLinker.Domain.Entity;
using FleetLinker.Application.DTOs.Identity;
using MediatR;
namespace FleetLinker.Application.Queries.Roles
{
    public record GetRoleList : IRequest<IEnumerable<ApplicationRole>>;
}