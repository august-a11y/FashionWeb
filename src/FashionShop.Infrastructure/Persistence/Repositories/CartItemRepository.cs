using FashionShop.Application.Interfaces;
using FashionShop.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FashionShop.Infrastructure.Persistence.Repositories
{
    public class CartItemRepository : Repository<CartItem, Guid>, ICartItemRepository
    {
        public CartItemRepository(ApplicationDbContext dbContext) : base(dbContext)
        {
        }

        public async Task<bool> ClearPurchasedItemsAsync(Guid userId, List<Guid> variantIds, CancellationToken cancellationToken)
        {
            var rowAffected = await _dbContext.CartItems
                .Include(ci => ci.Cart)
                .Where(ci => ci.Cart.UserId == userId && variantIds.Contains(ci.VariantId))
                .ExecuteDeleteAsync(cancellationToken);
            return rowAffected > 0;
        }
    }
}
