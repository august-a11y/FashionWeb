namespace FashionShop.Application.CartService.DTO
{
    public class CartItemCreateDTO
    {
        public Guid ProductId { get; set; }
        public Guid VariantId { get; set; }
        public int Quantity { get; set; }
    }
}
