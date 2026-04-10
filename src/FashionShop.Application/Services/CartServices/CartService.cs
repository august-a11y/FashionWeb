using FashionShop.Application.Common.Interfaces;
using FashionShop.Application.Interfaces;
using FashionShop.Application.Services.CartServices.DTO;
using FashionShop.Application.Services.CartServices.Helper;
using FluentResults;

namespace FashionShop.Application.Services.CartServices
{
    public class CartService : ICartService
    {
        private readonly IRequestContext _userContext;
        private readonly IVariantRepository _variantRepo;
        private readonly ICartCacheRepository _cartCacheRepo;

        public CartService(IVariantRepository variantRepo, ICartCacheRepository cartCacheRepo, IRequestContext userContext)
        {
            _userContext = userContext;
            _variantRepo = variantRepo;
            _cartCacheRepo = cartCacheRepo;
        }

        private string GetCartKey()
        {
            return _userContext.UserId != null
                ? CartKeyHelper.GetCartKey(_userContext.UserId, null)
                : CartKeyHelper.GetCartKey(null, _userContext.SessionId);
        }

        public async Task<Result<bool>> AddItemToCartAsync(CartItemCreateDTO cartItemCreateDTO, CancellationToken cancellationToken)
        {
            if (cartItemCreateDTO.Quantity <= 0)
                return Result.Fail("Quantity must be greater than 0.");

            var variant = await _variantRepo.GetByIdWithProductAsync(cartItemCreateDTO.VariantId,
                cancellationToken);

            if (variant == null)
                return Result.Fail("Variant not found");

            if (variant.StockQuantity < cartItemCreateDTO.Quantity)
                return Result.Fail("Insufficient stock");

            var cartItem = new CartItemDTO
            {
                ProductId = variant.ProductId,
                VariantId = variant.Id,
                ProductName = variant.Product?.Name ?? string.Empty,
                Color = variant.Color,
                Size = variant.Size,
                ThumbnailUrl = variant.ThumbnailUrl,
                Price = variant.Price,
                Quantity = cartItemCreateDTO.Quantity
            };

            await _cartCacheRepo.AddItemAsync(GetCartKey(), cartItem, cancellationToken);
            return Result.Ok(true);
        }

        public async Task<Result<CartDTO>> GetCartAsync(CancellationToken cancellationToken)
        {
            var cart = await _cartCacheRepo.GetAsync(GetCartKey(), cancellationToken);
            return Result.Ok(cart);
        }

        public async Task<Result<bool>> RemoveItemFromCartAsync(Guid productId, Guid variantId, CancellationToken cancellationToken)
        {
            var cartKey = GetCartKey();
            var cart = await _cartCacheRepo.GetAsync(cartKey, cancellationToken);
            var item = cart.Items.FirstOrDefault(i => i.VariantId == variantId && i.ProductId == productId);

            if (item == null)
                return Result.Fail("Item not found in cart");

            await _cartCacheRepo.RemoveItemAsync(cartKey, productId, variantId, cancellationToken);
            return Result.Ok(true);
        }

        

        public async Task<Result<bool>> UpdateQuantityItemFromCartAsync(
            CartItemUpdateDTO cartItemUpdateDTO,
            CancellationToken cancellationToken)
        {
            if (cartItemUpdateDTO.Quantity == 0)
                return Result.Fail("Quantity must be different from 0.");

            var cartKey = GetCartKey();

            bool success;

            if (cartItemUpdateDTO.Quantity < 0)
            {
                success = await _cartCacheRepo.DecreaseItemAsync(
                    cartKey,
                    cartItemUpdateDTO.ProductId,
                    cartItemUpdateDTO.VariantId,
                    Math.Abs(cartItemUpdateDTO.Quantity),
                    cancellationToken);
            }
            else
            {
                success = await _cartCacheRepo.IncreaseItemAsync(
                    cartKey,
                    cartItemUpdateDTO.ProductId,
                    cartItemUpdateDTO.VariantId,
                    cartItemUpdateDTO.Quantity,
                    cancellationToken);
            }

            if (!success)
                return Result.Fail("Item not found in cart");

            return Result.Ok(true);
        }
    }
}
