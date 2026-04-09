using FashionShop.Domain.Entities;

namespace FashionShop.Application.Interfaces
{
    public interface ICartItemRepository : IRepository<CartItem>
    {
        public Task<bool> DeleteItemsByCartIdAsync(Guid userId, List<Guid> variantIds, CancellationToken cancellationToken);
    }
}
