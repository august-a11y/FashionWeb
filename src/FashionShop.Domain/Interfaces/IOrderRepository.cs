using FashionShop.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FashionShop.Domain.Interfaces
{
    public interface IOrderRepository : IRepository<Entities.Order, Guid>
    {
        Task<Order?> GetByIdWithItemsAsync(Guid id, CancellationToken cancellationToken);
        Task<IList<Order>?> GetByUserIdWithItemsAsync(Guid userId, CancellationToken cancellationToken);
    }
}
