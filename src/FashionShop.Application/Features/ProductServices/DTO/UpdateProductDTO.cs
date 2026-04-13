using Microsoft.AspNetCore.Http;

namespace FashionShop.Application.Services.ProductServices.DTO
{
    public class UpdateDetailsProductDTO
    {
        public string? Name { get; set; } = string.Empty;
        public string? Description { get; set; } = string.Empty;
        public decimal? Price { get; set; }
        public IFormFile? ThumbnailFile { get; set; }
    }
}
