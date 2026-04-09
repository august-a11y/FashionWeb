namespace FashionShop.Application.Services.ProductServices.DTO
{
    public class ProductResponseDTO
    {
    public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string ThumbnailUrl { get; set; } = string.Empty;
        public Guid CategoryId { get; set; }
    }
}
