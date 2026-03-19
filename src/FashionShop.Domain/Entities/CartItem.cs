using FashionShop.Domain.Common;

namespace FashionShop.Domain.Entities
{
    public class CartItem : BaseEntity
    {
        public Guid CartId { get; set; }

        public virtual Cart Cart { get; set; } = null!;

        public Guid ProductId { get; set; }

        public virtual Product Product { get; set; } = null!;

        public Guid VariantId { get; set; }

        public virtual Variant Variant { get; set; } = null!;

        public int Quantity { get; set; }
       
        public decimal LineTotal => Variant?.Price * Quantity ?? 0;
    }
}