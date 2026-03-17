using FashionShop.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FashionShop.Domain.Interfaces
{
    public interface IVariantRepository : IRepository<Variant, Guid>
    {
        Task<bool> DecreaseStockAsync(Guid productId, Guid variantId, int quantity, CancellationToken cancellationToken);
        Task<List<Variant>> GetByIdWithProductAsync(List<Guid> variantIds, CancellationToken cancellationToken);
        Task<List<Variant>> GetListByIdsWithProductAsync(List<Guid> variantIds, CancellationToken cancellationToken);
    }
}
