using FashionShop.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FashionShop.Domain.Interfaces
{
    public interface IVariantRepository : IRepository<ProductVariant, Guid>
    {
        Task<bool> DecreaseStockAsync(Guid productId, Guid variantId, int quantity, CancellationToken cancellationToken);
        Task<IList<ProductVariant>> GetByIdWithProductAsync(IEnumerable<Guid> variantIds, CancellationToken cancellationToken);
    }
}
