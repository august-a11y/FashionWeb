using FashionShop.Application.CartService.DTO;
using FluentResults;

namespace FashionShop.Application.CartService
{
    public interface ICartService
    {
        Task<Result<bool>> AddItemToCartAsync(CartItemCreateDTO cartItemCreateDTO, CancellationToken cancellationToken);
        Task<Result<bool>> DecreaseQuantityItemFromCartAsync(CartItemUpdateDTO cartItemUpdateDTO, CancellationToken cancellationToken);
        Task<Result<bool>> RemoveItemFromCartAsync(Guid productId, Guid variantId, CancellationToken cancellationToken);
        Task<Result<CartDTO>> GetCartAsync(CancellationToken cancellationToken);
    }
}
