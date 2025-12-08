using FashionShop.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FashionShop.Domain.Interfaces
{
    public interface ISystemConfigRepository
    {
        Task<List<SystemConfigs>> GetAllAsync();
        Task<SystemConfigs?> GetByIdAsync(int id);
        Task<int> AddAsync(SystemConfigs configs);

        Task UpdateAsync(SystemConfigs configs);
        Task DeleteAsync(int id);
    }
}
