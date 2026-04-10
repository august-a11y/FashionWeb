using System.ComponentModel.DataAnnotations;

namespace FashionShop.Application.Services.CartServices.DTO
{
    public class CartItemUpdateDTO
    {
        [Required]
        public Guid VariantId { get; set; }
        
        [Required]
        public Guid ProductId { get; set; }
        
        [Required]

        public int Quantity { get; set; }
    }
}
