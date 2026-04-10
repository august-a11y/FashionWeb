using Microsoft.AspNetCore.Http;

namespace FashionShop.Application.Services.ProductServices.DTO
{
    public class CreateProductDTO
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal BasePrice { get; set; }
        public int StockQuantity { get; set; }
        public IFormFile? ThumbnailFile { get; set; }
        public Guid CategoryId { get; set; }
    }
}
