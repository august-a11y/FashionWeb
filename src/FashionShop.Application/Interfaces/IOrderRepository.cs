using FashionShop.Domain.Entities;

namespace FashionShop.Application.Interfaces
{
    public interface IOrderRepository : IRepository<Order, Guid>
    {
        Task<Order?> GetByIdWithItemsAsync(Guid id, CancellationToken cancellationToken);
        Task<IList<Order>?> GetByUserIdWithItemsAsync(Guid userId, CancellationToken cancellationToken);
    }
}
