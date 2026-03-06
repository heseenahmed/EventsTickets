using AutoMapper;
using MediatR;
using Tickets.Application.DTOs.Event;
using Tickets.Application.DTOs.EventOwner;
using Tickets.Domain.IRepository;

namespace Tickets.Application.Queries.EventOwner
{
    public class EventOwnerQueryHandler :
        IRequestHandler<GetAllEventOwnersQuery, List<EventOwnerDto>>,
        IRequestHandler<GetEventsByOwnerQuery, List<EventDto>>
    {
        private readonly IUserRepository _userRepository;
        private readonly IEventRepository _eventRepository;
        private readonly IMapper _mapper;

        public EventOwnerQueryHandler(
            IUserRepository userRepository,
            IEventRepository eventRepository,
            IMapper mapper)
        {
            _userRepository = userRepository;
            _eventRepository = eventRepository;
            _mapper = mapper;
        }

        public async Task<List<EventOwnerDto>> Handle(GetAllEventOwnersQuery request, CancellationToken cancellationToken)
        {
            var eventOwners = await _userRepository.GetUsersByRoleAsync("EventOwner", cancellationToken);

            return eventOwners.Select(user => new EventOwnerDto
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                OwnedEventsCount = user.OwnedEvents?.Count ?? 0
            }).ToList();
        }

        public async Task<List<EventDto>> Handle(GetEventsByOwnerQuery request, CancellationToken cancellationToken)
        {
            var events = await _eventRepository.GetEventsByOwnerIdAsync(request.UserId);
            return _mapper.Map<List<EventDto>>(events);
        }
    }
}
