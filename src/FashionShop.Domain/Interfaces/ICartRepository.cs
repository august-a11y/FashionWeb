using FashionShop.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FashionShop.Domain.Interfaces
{
    public interface ICartRepository : IRepository<Cart, Guid>
    {
        public Task<Cart?> GetCartWithItemsByUserIdAsync(Guid? userId , CancellationToken cancellationToken);
        
    }
}
