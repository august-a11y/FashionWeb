using FashionShop.Application.Services.CartServices.DTO;

namespace FashionShop.Application.Common.Interfaces
{
    public interface ICartCacheRepository
    {
        Task<CartDTO> GetAsync(string cartKey, CancellationToken cancellationToken);
        Task SetCartAsync(string cartKey, CartDTO cart, CancellationToken cancellationToken);
        Task AddItemAsync(string cartKey, CartItemDTO item, CancellationToken cancellationToken);
        Task RemoveItemAsync(string cartKey, Guid productId, Guid variantId, CancellationToken cancellationToken);
        Task<bool> DecreaseItemAsync(string cartKey, Guid productId, Guid variantId, int quantity, CancellationToken cancellationToken);
    }
}
