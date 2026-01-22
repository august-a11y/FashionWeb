using FashionShop.Domain.Common;
using System.ComponentModel.DataAnnotations.Schema;

namespace FashionShop.Domain.Entities
{
    public class CartItem : BaseEntity
    {
        public Guid CartId { get; set; }
        [ForeignKey("CartId")]
        public virtual Cart Cart { get; set; } = null!;
        public Guid ProductId { get; set; }
        [ForeignKey("ProductId")]
        public virtual Product Product { get; set; } = null!;

        public Guid ProductVariantId { get; set; }
        [ForeignKey("ProductVariantId")]
        public virtual ProductVariant ProductVariant { get; set; } = null!;
        public int Quantity { get; set; }
    }
}