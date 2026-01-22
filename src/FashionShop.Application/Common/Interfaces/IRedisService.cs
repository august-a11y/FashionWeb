using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FashionShop.Application.Common.Interfaces
{
    public interface IRedisService
    {
        Task HashSetAsync<T>(string key, string field, T value);

        
        Task<Dictionary<string, T>> HashGetAllAsync<T>(string key);

        
        Task HashDeleteAsync(string key, string field);

        
        Task DeleteKeyAsync(string key);

        
        Task SetExpireAsync(string key, TimeSpan timeSpan);
        Task<T?> HashGetAsync<T>(string key, string field);
    }
}
