using Microsoft.AspNetCore.Http;

namespace FashionShop.Application.Services.VariantServices.DTO
{
    public class UpdateVariantDTO
    {
        public string? Size { get; set; }
        public string? Color { get; set; }
        public IFormFile? ThumbnailFile { get; set; }
        public decimal? Price { get; set; }
        public int? StockQuantity { get; set; }
    }
}