using System.ComponentModel.DataAnnotations;
using FashionShop.Domain.Entities;

namespace FashionShop.Application.OrderService.DTO
{
    public class OrderDTO
    {
        public string OrderCode { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? ExpectedDeliveryDate { get; set; }
        public decimal SubTotal { get; set; }
        public decimal ShippingFee { get; set; }
        public decimal GrandTotal { get; set; }
        public string Note { get; set; } = string.Empty;
        public PaymentMethod PaymentMethod { get; set; }
        public PaymentStatus PaymentStatus { get; set; }
        public ShipmentStatus ShippingStatus { get; set; }
        public OrderStatus Status { get; set; }
        public List<OrderItemDTO> Items { get; set; } = new();
        public AddressDTO ShippingAddress { get; set; } = new();
    }

    public class OrderItemDTO
    {
        public Guid VariantId { get; set; }
        public Guid ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public string ThumbnailUrl { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
        public string Size { get; set; } = string.Empty;

        public decimal LineTotal => UnitPrice * Quantity;
    }

    public class AddressDTO
    {
        [Required(ErrorMessage = "Full name is required")]
        public string FullName { get; set; } = string.Empty;

        [Phone]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required]
        public string StreetLine { get; set; } = string.Empty;

        public string Ward { get; set; } = string.Empty;

        [Required]
        public string District { get; set; } = string.Empty;

        [Required]
        public string City { get; set; } = string.Empty;
    }
}
