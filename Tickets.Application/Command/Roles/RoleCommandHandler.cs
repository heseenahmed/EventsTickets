using AutoMapper;
using FleetLinker.Application.Common.Localization;
using FleetLinker.Domain.Entity;
using FleetLinker.Domain.Models;
using FleetLinker.Domain.IRepository;
using MediatR;

namespace FleetLinker.Application.Command.Roles
{
    public class RoleCommandHandler :
        IRequestHandler<AddRoleCommand, bool>,
        IRequestHandler<DeleteRoleCommand, bool>,
        IRequestHandler<UpdateRoleCommand, bool>
    {
        private readonly IMapper _mapper;
        private readonly IRoleRepository _roleRepository;
        private readonly IAppLocalizer _localizer;
        public RoleCommandHandler(IMapper mapper, IRoleRepository roleRepository , IAppLocalizer localizer)
        {
            _mapper = mapper;
            _roleRepository = roleRepository;
            _localizer = localizer;
        }
        public async Task<bool> Handle(AddRoleCommand request, CancellationToken cancellationToken)
        {
            if (request?.Role == null)
                throw new ArgumentException("Role payload is required.");
            var newRole = _mapper.Map<ApplicationRole>(request.Role);
            if (string.IsNullOrWhiteSpace(newRole?.Name))
                throw new ArgumentException(_localizer[LocalizationMessages.RoleNameRequired]);
            newRole.Id = Guid.NewGuid().ToString();
            newRole.NormalizedName = newRole.Name.ToUpperInvariant();
            var created = await _roleRepository.AddRoleAsync(newRole);
            if (!created)
                throw new InvalidOperationException(_localizer[LocalizationMessages.ErrorAddingRole]);
            return true;
        }
        public async Task<bool> Handle(DeleteRoleCommand request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.RoleName))
                throw new ArgumentException(_localizer[LocalizationMessages.RoleNameRequired]);
            var deleted = await _roleRepository.DeleteRoleByNameAsync(request.RoleName);
            if (!deleted)
                throw new KeyNotFoundException(_localizer[LocalizationMessages.RoleNotFound]);
            return true;
        }
        public async Task<bool> Handle(UpdateRoleCommand request, CancellationToken cancellationToken)
        {
            if (request?.Role == null)
                throw new ArgumentException(_localizer[LocalizationMessages.RoleNameRequired]);
            if (string.IsNullOrWhiteSpace(request.Role.Name))
                throw new ArgumentException(_localizer[LocalizationMessages.RoleNameRequired]);
            var updated = await _roleRepository.UpdateRoleAsync(request.Role);
            if (!updated)
                throw new InvalidOperationException(_localizer[LocalizationMessages.ErrorUpdateRole]);
            return true;
        }
    }
}
