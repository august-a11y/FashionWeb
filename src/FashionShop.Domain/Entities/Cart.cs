using FashionShop.Domain.Common;
using FashionShop.Domain.Identity;

namespace FashionShop.Domain.Entities
{
    public class Cart : BaseEntity
    {
        public Guid? UserId { get; set; }
        public DateTime LastUpdate { get; set; } = DateTime.UtcNow;
        public virtual ICollection<CartItem> Items { get; set; } = new List<CartItem>();
        public virtual AppUser User { get; set; }

        public Cart(Guid? userId)
        {
            UserId = userId;
        }

        public void AddCartItem(Guid productId, Guid variantId, int quantity)
        {
            var existingItem = Items.FirstOrDefault(i => i.VariantId == variantId && i.ProductId == productId);
            
            if (existingItem != null)
            {
                existingItem.Quantity += quantity; 
            }
            else
            {
                Items.Add(new CartItem
                {
                    CartId = this.Id,
                    ProductId = productId,
                    VariantId = variantId,
                    Quantity = quantity
                });
            }
            
            LastUpdate = DateTime.UtcNow;
        }

        public void RemoveCartItem(Guid variantId, Guid productId)
        {
            var item = Items.FirstOrDefault(i => i.VariantId == variantId && i.ProductId == productId);
            if (item != null)
            {
                Items.Remove(item);
                LastUpdate = DateTime.UtcNow;
            }
        }

        public void DecreaseQuantityItem(Guid variantId, Guid productId, int quantity)
        {
            var item = Items.FirstOrDefault(i => i.VariantId == variantId && i.ProductId == productId);
            if (item != null)
            {
                item.Quantity = Math.Max(0, item.Quantity - quantity);
                if (item.Quantity == 0)
                    Items.Remove(item);
                LastUpdate = DateTime.UtcNow;
            }
        }

        public void UpdateCartItemQuantity(Guid variantId, Guid productId, int newQuantity)
        {
            var item = Items.FirstOrDefault(i => i.VariantId == variantId && i.ProductId == productId);
            if (item != null && newQuantity > 0)
            {
                item.Quantity = newQuantity;
                LastUpdate = DateTime.UtcNow;
            }
        }
    }
}
