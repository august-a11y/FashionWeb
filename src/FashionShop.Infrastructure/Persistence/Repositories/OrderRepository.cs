using FashionShop.Application.Interfaces;
using FashionShop.Domain.Entities;
using Microsoft.EntityFrameworkCore;

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
                .Include(o => o.Payment)
                .Include(o => o.Shipment)
                .Where(o => o.Id == id)
                .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<IList<Order>?> GetByUserIdWithItemsAsync(Guid userId, CancellationToken cancellationToken)
        {
            return await _dbContext.Orders
                .Include(o => o.OrderItems)
                .Include(o => o.Payment)
                .Include(o => o.Shipment)
                .Where(o => o.UserId == userId)
                .ToListAsync(cancellationToken);
        }
    }
}
