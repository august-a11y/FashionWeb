using FashionShop.Domain.Entities;
using System.ComponentModel.DataAnnotations;

namespace FashionShop.Application.OrderService.DTO
{
    public class CreateOrderDTO
    {
        [Required(ErrorMessage = "Order items are required")]
        [MinLength(1, ErrorMessage = "Order must contain at least one item")]
        public List<OrderItemDTO> OrderItems { get; set; } = new List<OrderItemDTO>();

        [Required(ErrorMessage = "Shipping address is required")]
        public AddressDTO ShippingAddress { get; set; } = new AddressDTO();

        [Required(ErrorMessage = "Phone number is required")]
        [Phone(ErrorMessage = "Invalid phone number")]
        public string PhoneNumber { get; set; } = string.Empty;

        [MaxLength(500, ErrorMessage = "Note cannot exceed 500 characters")]
        public string Note { get; set; } = string.Empty;

        [Required(ErrorMessage = "Payment method is required")]
        public PaymentMethod PaymentMethod { get; set; }
    }
}