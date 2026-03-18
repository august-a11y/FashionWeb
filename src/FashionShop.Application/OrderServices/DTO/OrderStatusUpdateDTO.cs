using System.ComponentModel.DataAnnotations;

namespace FashionShop.Application.OrderServices.DTO
{
    public class OrderStatusUpdateDTO
    {
        [Required(ErrorMessage = "Status is required")]
        public string Status { get; set; } = string.Empty;
    }
}