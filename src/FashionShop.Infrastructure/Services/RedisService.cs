using FashionShop.Application.Common.Interfaces;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace FashionShop.Infrastructure.Services
{
    public class RedisService : IRedisService
    {
        private readonly IConnectionMultiplexer _redis;
        private readonly IDatabase _db;

        public RedisService(IConnectionMultiplexer redis)
        {
            _redis = redis;
            _db = redis.GetDatabase();
        }

        public async Task HashSetAsync<T>(string key, string field, T value)
        {
            var json = JsonSerializer.Serialize(value);
            await _db.HashSetAsync(key, field, json);
        }

        public async Task<Dictionary<string, T>> HashGetAllAsync<T>(string key)
        {
            var hashEntries = await _db.HashGetAllAsync(key);
            var result = new Dictionary<string, T>();

            foreach (var entry in hashEntries)
            {
                if (entry.Value.HasValue)
                {
                    var val = JsonSerializer.Deserialize<T>(entry.Value.ToString());
                    result.Add(entry.Name.ToString(), val);
                }
            }
            return result;
        }

        public async Task HashDeleteAsync(string key, string field)
        {
            await _db.HashDeleteAsync(key, field);
        }

        public async Task DeleteKeyAsync(string key)
        {
            await _db.KeyDeleteAsync(key);
        }

        public async Task SetExpireAsync(string key, TimeSpan timeSpan)
        {
            await _db.KeyExpireAsync(key, timeSpan);
        }

        public async Task<T?> HashGetAsync<T>(string key, string field)
        {
            RedisValue redisValue = await _db.HashGetAsync(key, field);
            if(redisValue.IsNullOrEmpty)
            {
                return default;
            }
            return JsonSerializer.Deserialize<T>(redisValue.ToString());

        }
    }
}
