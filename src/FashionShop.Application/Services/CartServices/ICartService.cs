using FashionShop.Application.Services.CartServices.DTO;
using FluentResults;

namespace FashionShop.Application.Services.CartServices
{
    public interface ICartService
    {
        Task<Result<bool>> AddItemToCartAsync(CartItemCreateDTO cartItemCreateDTO, CancellationToken cancellationToken);
        Task<Result<bool>> DecreaseQuantityItemFromCartAsync(CartItemUpdateDTO cartItemUpdateDTO, CancellationToken cancellationToken);
        Task<Result<bool>> RemoveItemFromCartAsync(Guid productId, Guid variantId, CancellationToken cancellationToken);
        Task<Result<CartDTO>> GetCartAsync(CancellationToken cancellationToken);
    }
}
