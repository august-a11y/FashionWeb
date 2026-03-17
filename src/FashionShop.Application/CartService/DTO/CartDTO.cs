namespace FashionShop.Application.CartService.DTO
{
    public class CartDTO
    {
        public int TotalItems { get; set; }
        public decimal TotalPrice { get; set; }
        public List<CartItemDTO> Items { get; set; } = new List<CartItemDTO>();
    }
}
