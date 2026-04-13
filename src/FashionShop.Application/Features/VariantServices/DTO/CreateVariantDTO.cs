using Microsoft.AspNetCore.Http;

namespace FashionShop.Application.Services.VariantServices.DTO
{
    public class CreateVariantDTO
    {
        public Guid ProductId { get; set; }
        public string Size { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
        public IFormFile? ThumbnailFile { get; set; }
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
    }
}