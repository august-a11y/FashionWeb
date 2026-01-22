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
    public class CartItemRepository : Repository<CartItem, Guid>, ICartItemRepository
    {
        public CartItemRepository(ApplicationDbContext dbContext) : base(dbContext)
        {
        }

        public async Task<bool> ClearPurchasedItemsAsync(Guid userId, IList<Guid> variantIds, CancellationToken cancellationToken)
        {
            var rowAffected = await _dbContext.CartItems
                .Include(ci => ci.Cart)
                .Where(ci => ci.Cart.UserId == userId && variantIds.Contains(ci.ProductVariantId))
                .ExecuteDeleteAsync(cancellationToken);
            return rowAffected > 0;
        }
    }
}
