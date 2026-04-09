using FashionShop.Application.Common.Interfaces;
using FashionShop.Application.Interfaces;
using FashionShop.Application.Services.CartServices.DTO;
using FashionShop.Application.Services.CartServices.Helper;
using StackExchange.Redis;
using System.Text.Json;

namespace FashionShop.Infrastructure.Cache
{
    public class CartCacheRepository : ICartCacheRepository
    {
        private readonly IDatabase _redis;
        private readonly ICartRepository _cartRepository;
        

        public CartCacheRepository(IConnectionMultiplexer redis, ICartRepository cartRepository)
        {
            _cartRepository = cartRepository;
            _redis = redis.GetDatabase();

        }

        public async Task<CartDTO> GetAsync(string cartKey, CancellationToken cancellationToken)
        {
            Guid userId = cartKey.StartsWith("user:") ? Guid.Parse(cartKey.Substring(5)) : Guid.Empty;
            var hashEntries = await _redis.HashGetAllAsync(cartKey);
            var cartDto = new CartDTO();
            if (hashEntries.Length == 0)
            {
                var cart = await _cartRepository.GetCartWithItemsByUserIdAsync(userId, cancellationToken);
                if (cart == null) return cartDto;
                foreach(var item in cart.Items)
                {
                    cartDto.Items.Add(new CartItemDTO
                    {
                        ProductId = item.ProductId,
                        VariantId = item.VariantId,
                        ProductName = item.Product.Name,
                        Color = item.Variant.Color,
                        Size = item.Variant.Size,
                        ThumbnailUrl = item.Variant.ThumbnailUrl,
                        Quantity = item.Quantity,
                        Price = item.LineTotal / item.Quantity
                    });
                    cartDto.TotalItems += item.Quantity;
                    cartDto.TotalPrice += item.LineTotal;
                }
                await SetCartAsync(cartKey, cartDto, cancellationToken);
            }
            else
            {
                foreach (var entry in hashEntries)
                {
                    if (entry.Value.HasValue)
                    {
                        var item = JsonSerializer.Deserialize<CartItemDTO>(entry.Value.ToString());
                        if (item != null) cartDto.Items.Add(item);
                    }
                }

                cartDto.TotalItems = cartDto.Items.Sum(i => i.Quantity);
                cartDto.TotalPrice = cartDto.Items.Sum(i => i.Quantity * i.Price);
            }
            return cartDto;
        }

        public async Task AddItemAsync(string cartKey, CartItemDTO item, CancellationToken cancellationToken)
        {


            var fieldKey = CartKeyHelper.GetItemFieldKey(item.ProductId, item.VariantId);
            var existingValue = await _redis.HashGetAsync(cartKey, fieldKey);

            if (existingValue.HasValue)
            {
                var existingItem = JsonSerializer.Deserialize<CartItemDTO>(existingValue.ToString());
                if (existingItem != null)
                {
                    item.Quantity += existingItem.Quantity;
                }
            }

            var jsonValue = JsonSerializer.Serialize(item);
            await _redis.HashSetAsync(cartKey, fieldKey, jsonValue);
            await _redis.KeyExpireAsync(cartKey, TimeSpan.FromDays(30));
        }

        public async Task RemoveItemAsync(string cartKey, Guid productId, Guid variantId, CancellationToken cancellationToken)
        {


            var fieldKey = CartKeyHelper.GetItemFieldKey(productId, variantId);
            await _redis.HashDeleteAsync(cartKey, fieldKey);
        }

        public async Task<bool> DecreaseItemAsync(string cartKey, Guid productId, Guid variantId, int quantity, CancellationToken cancellationToken)
        {


            var fieldKey = CartKeyHelper.GetItemFieldKey(productId, variantId);
            var existingValue = await _redis.HashGetAsync(cartKey, fieldKey);

            if (!existingValue.HasValue)
                return false;

            var item = JsonSerializer.Deserialize<CartItemDTO>(existingValue.ToString());
            if (item == null)
                return false;

            item.Quantity -= quantity;

            if (item.Quantity <= 0)
            {
                await _redis.HashDeleteAsync(cartKey, fieldKey);
            }
            else
            {
                var jsonValue = JsonSerializer.Serialize(item);
                await _redis.HashSetAsync(cartKey, fieldKey, jsonValue);
            }

            return true;
        }

        public async Task SetCartAsync(string cartKey, CartDTO cart, CancellationToken cancellationToken)
        {


            // Clear existing hash before overwriting
            await _redis.KeyDeleteAsync(cartKey);

            foreach (var item in cart.Items)
            {
                var fieldKey = CartKeyHelper.GetItemFieldKey(item.ProductId, item.VariantId);
                var jsonValue = JsonSerializer.Serialize(item);
                await _redis.HashSetAsync(cartKey, fieldKey, jsonValue);
            }

            await _redis.KeyExpireAsync(cartKey, TimeSpan.FromDays(30));
        }
    }
}
