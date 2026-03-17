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
    public class VariantRepository : Repository<Variant, Guid>, IVariantRepository 
    {
        public VariantRepository(ApplicationDbContext dbContext) : base(dbContext)
        {
        }

        public async Task<bool> DecreaseStockAsync(Guid productId, Guid variantId, int quantity, CancellationToken cancellationToken)
        {
            int rowsAffected = await _dbContext.ProductVariants.Where(p => p.ProductId == productId && p.Id == variantId && p.StockQuantity >= quantity)
                .ExecuteUpdateAsync(p => p
                    .SetProperty(v => v.StockQuantity, v => v.StockQuantity - quantity)
                    .SetProperty(v => v.UpdatedAt, v => DateTime.UtcNow),
                    cancellationToken
                    );
            return rowsAffected > 0;
        }

        public async Task<List<Variant>> GetByIdWithProductAsync(List<Guid> variantIds, CancellationToken cancellationToken)
        {
            return await _dbContext.ProductVariants
                                            .Include(v => v.Product)
                                            .Where(v => variantIds.Contains(v.Id))
                                            .ToListAsync(cancellationToken);

                                                            
        }

        public async Task<List<Variant>> GetListByIdsWithProductAsync(List<Guid> variantIds, CancellationToken cancellationToken)
        {
            return await _dbContext.ProductVariants
                                    .Include(v => v.Product)
                                    .Where(v => variantIds.Contains(v.Id))
                                    .ToListAsync(cancellationToken);
        }
    }
}
