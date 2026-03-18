using System.ComponentModel.DataAnnotations;

namespace FashionShop.Application.CartServices.DTO
{
    public class CartItemUpdateDTO
    {
        [Required]
        public Guid VariantId { get; set; }
        
        [Required]
        public Guid ProductId { get; set; }
        
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be greater than 0")]
        public int Quantity { get; set; }
    }
}
