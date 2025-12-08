

using FashionShop.Domain.Common;

namespace FashionShop.Domain.Entities
{
    public class Product : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }

        public string ImageUrl { get; set; } = string.Empty;
        public string Size { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;

        public int StockQuantity { get; set; }

        public int CategoryId { get; set; }
        public Category? Category { get; set; }

    }
}
