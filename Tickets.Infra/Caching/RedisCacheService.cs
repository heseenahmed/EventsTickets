
using FleetLinker.Application.Common.Caching;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace FleetLinker.Infra.Caching
{
    public class RedisCacheService : ICacheService
    {
        private readonly IDistributedCache _cache;

        public RedisCacheService(IDistributedCache cache)
        {
            _cache = cache;
        }
        public async Task<T?> GetAsync<T>(string key, CancellationToken ct)
        {
            var json = await _cache.GetStringAsync(key, ct);
            if (string.IsNullOrWhiteSpace(json)) return default;
            return JsonSerializer.Deserialize<T>(json);
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan ttl, CancellationToken ct)
        {
            var json = JsonSerializer.Serialize(value);
            await _cache.SetStringAsync(
                key,
                json,
                new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = ttl
                },
                ct);
        }
        public Task RemoveAsync(string key, CancellationToken ct)
            => _cache.RemoveAsync(key, ct);
    }
}
