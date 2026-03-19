using FashionShop.Domain.Entities;

namespace FashionShop.Application.Interfaces
{
    public interface ICartItemRepository : IRepository<CartItem, Guid>
    {
        public Task<bool> ClearPurchasedItemsAsync(Guid userId, List<Guid> variantIds, CancellationToken cancellationToken);
    }
}
