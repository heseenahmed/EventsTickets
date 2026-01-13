using AutoMapper;
using Tickets.Application.DTOs.Identity;
using Tickets.Domain.IRepository;
using Tickets.Application.DTOs.User;
using Tickets.Domain.Models;
using MediatR;
using Tickets.Application.Common.Caching;
namespace Tickets.Application.Queries.User
{
    public class UserQueryHandler :
        IRequestHandler<GetUserInfoAsyncCommand, UserInfoAPI?>,
        IRequestHandler<GetUserById, UserForListDto>,
        IRequestHandler<GetAllUser, List<UserForListDto>>
    {
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;
        private readonly ICacheService _cache;
        public UserQueryHandler(IUserRepository userRepository, IMapper mapper , ICacheService cache)
        {
            _userRepository = userRepository;
            _mapper = mapper;
            _cache = cache;
        }
        public async Task<UserInfoAPI?> Handle(GetUserInfoAsyncCommand request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.Id))
                throw new ArgumentException("User ID cannot be empty.");
            var userInfo = await _userRepository.GetUserInfoAsync(request.Id)
                           ?? throw new KeyNotFoundException("User not found with the given ID.");
            return userInfo;
        }
        public async Task<UserForListDto> Handle(GetUserById request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.Id))
                throw new ArgumentException("User ID is required.");
            var user = await _userRepository.GetByIdAsync(request.Id)
                       ?? throw new KeyNotFoundException("User not found.");
            return _mapper.Map<UserForListDto>(user);
        }
        public async Task<List<UserForListDto>> Handle(GetAllUser request, CancellationToken cancellationToken)
        {
            // 1?? Try cache first
            var cachedUsers = await _cache.GetAsync<List<UserForListDto>>(
                CacheKeys.UsersAll,
                cancellationToken);

            if (cachedUsers != null)
                return cachedUsers;

            // 2?? Load from DB
            var users = await _userRepository.GetAllAsync()
                        ?? new List<Domain.Entity.ApplicationUser>();

            var result = _mapper.Map<List<UserForListDto>>(users)
                         ?? new List<UserForListDto>();

            // 3?? Store in cache (TTL = 5 minutes)
            await _cache.SetAsync(
                CacheKeys.UsersAll,
                result,
                TimeSpan.FromMinutes(5),
                cancellationToken);

            return result;
        }
    }
}
