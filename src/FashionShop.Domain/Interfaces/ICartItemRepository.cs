using FashionShop.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FashionShop.Domain.Interfaces
{
    public interface ICartItemRepository : IRepository<CartItem, Guid>
    {
        public Task<bool> ClearPurchasedItemsAsync(Guid userId, List<Guid> variantIds, CancellationToken cancellationToken);
    }
}
