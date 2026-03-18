namespace FashionShop.Application.CartServices.DTO
{
    public record CartItemDTO
    {
        public Guid ProductId { get; set; }
        public Guid VariantId { get; set; }

        public string ProductName { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
        public string Size { get; set; } = string.Empty;
        public string ThumbnailUrl { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public bool IsValid { get; set; } = true;
        public string StockStatus { get; set; } = string.Empty;

        public decimal LineTotal => Price * Quantity;

        public decimal TotalPrice
        {
            get => LineTotal;
            set {}
        }
    }
}
