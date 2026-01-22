

using FashionShop.Domain.Common;
using System.Runtime.CompilerServices;

namespace FashionShop.Domain.Entities
{
    public class Product : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal? BasePrice { get; set; }

        public string ThumbnailUrl { get; set; } = string.Empty;
        public Guid CategoryId { get; set; }
        public Category? Category { get; set; }
        public ICollection<ProductVariant> Variants { get; set; } = new List<ProductVariant>();
        public void UpdateDetails(string name, string desc, string img, decimal? basePrice)
        {

            Name = !string.IsNullOrEmpty(name) ? name : Name;
            BasePrice = basePrice != 0 ? basePrice : BasePrice;
            Description = !string.IsNullOrEmpty(desc) ? desc : Description;
            ThumbnailUrl = !string.IsNullOrEmpty(img) ? img : ThumbnailUrl;
        }

    }

    public class ProductVariant : BaseEntity
    {
        public Guid ProductId { get; set; }
        public virtual Product Product { get; set; } = null!;
        public string Sku { get; set; } = string.Empty;
        public string Size { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
        public string ThumbnailUrl { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
        public void UpdateDetails(string sku, string size, decimal? price, string collor, int? stock)
        {

            Sku = !string.IsNullOrEmpty(sku) ? sku : Sku;
            Size = !string.IsNullOrEmpty(size) ? size : Size;
            Price = price ?? Price;
            StockQuantity = stock ?? StockQuantity;
            Color = !string.IsNullOrEmpty(collor) ? collor : Color;
        }
    }
}
