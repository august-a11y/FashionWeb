using FashionShop.Domain.Entities;
using FashionShop.Domain.Interfaces;
using FashionShop.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FashionShop.Infrastructure.Persistence.Repositories
{
    public class OrderRepository : Repository<Order, Guid>, IOrderRepository
    {
        public OrderRepository(ApplicationDbContext dbContext) : base(dbContext)
        {
        }

        public async Task<Order?> GetByIdWithItemsAsync(Guid id, CancellationToken cancellationToken)
        {
            return await _dbContext.Orders
                .Include(o => o.OrderItems)
                .Where(o => o.Id == id)
                .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<IList<Order>?> GetByUserIdWithItemsAsync(Guid userId, CancellationToken cancellationToken)
        {
            return await _dbContext.Orders
                .Include(o => o.OrderItems)
                .Where(o => o.UserId == userId)
                .ToListAsync(cancellationToken);
        }
    }
}
