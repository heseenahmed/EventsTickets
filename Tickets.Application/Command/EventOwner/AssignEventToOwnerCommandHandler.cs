using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Tickets.Application.Common.Localization;
using Tickets.Application.DTOs;
using Tickets.Domain.Entity;
using Tickets.Domain.IRepository;

namespace Tickets.Application.Command.EventOwner
{
    public class AssignEventToOwnerCommandHandler : IRequestHandler<AssignEventToOwnerCommand, APIResponse<bool>>
    {
        private readonly IEventRepository _eventRepository;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IAppLocalizer _localizer;

        public AssignEventToOwnerCommandHandler(
            IEventRepository eventRepository,
            UserManager<ApplicationUser> userManager,
            IAppLocalizer localizer)
        {
            _eventRepository = eventRepository;
            _userManager = userManager;
            _localizer = localizer;
        }

        public async Task<APIResponse<bool>> Handle(AssignEventToOwnerCommand request, CancellationToken cancellationToken)
        {
            // Validate user exists
            var user = await _userManager.FindByIdAsync(request.UserId);
            if (user == null)
                return APIResponse<bool>.Fail(StatusCodes.Status404NotFound, null, _localizer[LocalizationMessages.UserNotFound]);

            // Validate user has EventOwner role
            var isEventOwner = await _userManager.IsInRoleAsync(user, "EventOwner");
            if (!isEventOwner)
                return APIResponse<bool>.Fail(StatusCodes.Status400BadRequest, null, _localizer[LocalizationMessages.UserIsNotEventOwner]);

            // Validate event exists
            var eventEntity = await _eventRepository.GetByGuidAsync(request.EventId);
            if (eventEntity == null)
                return APIResponse<bool>.Fail(StatusCodes.Status404NotFound, null, _localizer[LocalizationMessages.EventNotFound]);

            // Assign owner to event
            eventEntity.OwnerId = request.UserId;
            await _eventRepository.UpdateAsync(eventEntity);

            return APIResponse<bool>.Success(true, _localizer[LocalizationMessages.EventAssignedToOwnerSuccessfully]);
        }
    }
}
