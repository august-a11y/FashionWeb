using FashionShop.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FashionShop.Domain.Dto
{
    public class OrderDetailDto
    {
        public Guid Id { get; set; }
        public string OrderCode { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public string Status { get; set; } = string.Empty;
        public string PaymentStatus { get; set; } = string.Empty;

        public decimal TotalAmount { get; set; }
        public Address ShippingAddress { get; set; } = new Address();


        public List<OrderLineItemDto> Items { get; set; } = new List<OrderLineItemDto>();
    }

    public class OrderLineItemDto
    {
        public Guid ProductId { get; set; }
        public Guid VariantId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
        public string Size { get; set; } = string.Empty;
        public decimal UnitPrice { get; set; } 
        public int Quantity { get; set; }
        public string ThumbnailUrl { get; set; } = string.Empty;
    }
}
