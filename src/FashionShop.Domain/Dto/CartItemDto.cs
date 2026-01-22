using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FashionShop.Domain.Dto
{
    public record CartItemDto
    {
        public Guid ProductId { get; set; }
        public Guid VariantId { get; set; }

        public string ProductName { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
        public string Size { get; set; } = string.Empty;
        public string ThumbnailUrl { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public decimal? OriginalPrice { get; set; }
        public int StockQuantity { get; set; }
        public decimal LineTotal => Price * Quantity;
        public bool IsValid { get; set; } = true;
        public int Quantity { get; set; }
        public string StockStatus { get; set; } = string.Empty;
    }
}
