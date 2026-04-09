using System.ComponentModel.DataAnnotations;

namespace FashionShop.Application.Services.CartServices.DTO
{
    public class CartItemRemoveDTO
    {
        [Required]
        public Guid VariantId { get; set; }

        [Required]
        public Guid ProductId { get; set; }
    }
}