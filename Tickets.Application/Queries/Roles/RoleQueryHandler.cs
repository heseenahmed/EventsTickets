using AutoMapper;
using Tickets.Domain.Entity;
using Tickets.Application.DTOs.Identity;
using Tickets.Domain.IRepository;
using MediatR;
using System.Linq;
namespace Tickets.Application.Queries.Roles.Queries
{
    public class GetRoleQueryHandler : IRequestHandler<GetRoleList, IEnumerable<ApplicationRole>>
    {
        private readonly IMapper _mapper;
        private readonly IRoleRepository _roleRepository;
        public GetRoleQueryHandler(IRoleRepository repository, IMapper mapper)
        {
            _roleRepository = repository;
            _mapper = mapper;
        }
        public async Task<IEnumerable<ApplicationRole>> Handle(GetRoleList request, CancellationToken cancellationToken)
        {
            var roles = await _roleRepository.GetRolesAsync();
            return roles ?? Enumerable.Empty<ApplicationRole>();
        }
    }
}
