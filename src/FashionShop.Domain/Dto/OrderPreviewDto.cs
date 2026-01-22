using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FashionShop.Domain.Dto
{
    public class OrderPreviewDto
    {
        public decimal Subtotal { get; set; }
        public decimal ShippingFee { get; set; }
        public decimal TotalAmount { get; set; }
        public List<OrderLineItemDto> OrderItems { get; set; } = new List<OrderLineItemDto>();
    }
}
