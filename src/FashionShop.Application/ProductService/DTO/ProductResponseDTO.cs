namespace FashionShop.Application.ProductService.DTO
{
    public class ProductResponseDTO
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string ThumbnailUrl { get; set; } = string.Empty;
        public Guid CategoryId { get; set; }
    }
}
