using FashionShop.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FashionShop.Infrastructure.Services
{
    public class SystemConfigService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMemoryCache _cache;
        public SystemConfigService(ApplicationDbContext context, IMemoryCache cache)
        {
            _context = context;
            _cache = cache;
        }

        public async Task<string?> GetSystemConfigValueAsync(string key)
        {
            return await _cache.GetOrCreateAsync(key, async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10);
                var config = await _context.SystemConfigs
                    .Where(c => c.ConfigKey == key)
                    .FirstOrDefaultAsync();

                return config?.ConfigValue ?? "lost SystemConfig";

            });
        }
    }
}
