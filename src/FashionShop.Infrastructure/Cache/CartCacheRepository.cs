using FashionShop.Application.CartServices.DTO;
using FashionShop.Application.CartServices.Helper;
using FashionShop.Application.Common.Interfaces;
using StackExchange.Redis;
using System.Text.Json;

namespace FashionShop.Infrastructure.Cache
{
    public class CartCacheRepository : ICartCacheRepository
    {
        private readonly IDatabase _redis;

        public CartCacheRepository(IConnectionMultiplexer redis)
        {
            _redis = redis.GetDatabase();
        }

        public async Task<CartDTO> GetAsync(string cartKey, CancellationToken cancellationToken)
        {
            var hashEntries = await _redis.HashGetAllAsync(cartKey);
            var cart = new CartDTO();

            foreach (var entry in hashEntries)
            {
                if (entry.Value.HasValue)
                {
                    var item = JsonSerializer.Deserialize<CartItemDTO>(entry.Value.ToString());
                    if (item != null) cart.Items.Add(item);
                }
            }

            cart.TotalItems = cart.Items.Sum(i => i.Quantity);
            cart.TotalPrice = cart.Items.Sum(i => i.Quantity * i.Price);
            return cart;
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
