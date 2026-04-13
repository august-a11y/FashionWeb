using FashionShop.Application.Common.Interfaces;
using StackExchange.Redis;
using System.Text.Json;

namespace FashionShop.Infrastructure.Caching
{
    public class RedisCache : IRedisCache
    {
        private readonly JsonSerializerOptions _jsonOptions;
        private readonly IDatabase _db;

        public RedisCache(IConnectionMultiplexer redis)
        {
            
            _db = redis.GetDatabase();
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            };
        }

        public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
        {
            var value = await _db.StringGetAsync(key);

            if (value.IsNullOrEmpty) return default;

            return JsonSerializer.Deserialize<T>((string)value!, _jsonOptions);
        }

        public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
        {
            await _db.KeyDeleteAsync(key);
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan? expiry = null, CancellationToken cancellationToken = default)
        {
            var json = JsonSerializer.Serialize(value, _jsonOptions);
            await _db.StringSetAsync(key, json, expiry, When.Always);
        }
    }
}
