using FashionShop.Domain.Entities;
using FashionShop.Domain.Interfaces;
using FashionShop.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FashionShop.Infrastructure.Repositories
{
    public class SystemConfigsRepository : ISystemConfigRepository
    {
        private readonly ApplicationDbContext _context;

        public SystemConfigsRepository(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<int> AddAsync(SystemConfigs configs)
        {
            await _context.SystemConfigs.AddAsync(configs);
            await _context.SaveChangesAsync();
            return Task.FromResult(configs.Id).Result;
        }

        public async Task DeleteAsync(int id)
        {
            await _context.SystemConfigs.Where(c => c.Id == id).ExecuteDeleteAsync();
            await _context.SaveChangesAsync();
        }

        public async Task<List<SystemConfigs>> GetAllAsync()
        {
            var configs = await _context.SystemConfigs.ToListAsync();
            if (configs.Count == 0)
            {
                throw new KeyNotFoundException("No system configurations found.");
            }
            return Task.FromResult(configs).Result;

        }

        public async Task<SystemConfigs?> GetByIdAsync(int id)
        {
           var config = await _context.SystemConfigs.FirstOrDefaultAsync(c => c.Id == id);
            return Task.FromResult(config).Result;
        }

        public Task UpdateAsync(SystemConfigs configs)
        {
            var config = _context.SystemConfigs.FirstOrDefault(c => c.Id == configs.Id);
            config = new SystemConfigs { 
            
               
                ConfigKey = configs.ConfigKey,
                ConfigValue = configs.ConfigValue,
                Description = configs.Description
            };
            _context.SystemConfigs.Update(config);
            _context.SaveChangesAsync();
            return Task.CompletedTask;

        }
    }
}
