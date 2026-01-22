using FashionShop.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FashionShop.Domain.Dto
{
    public class OrderInputDto
    {
        public List<OrderItemDto> OrderItems { get; set; } = new List<OrderItemDto>();
        public AddressDto ShippingAddress { get; set; } = new AddressDto();
        public string PhoneNumber { get; set; } = string.Empty;
        public string Note { get; set; } = string.Empty;
        public PaymentMethod PaymentMethod { get; set; } = new PaymentMethod();
    }

    public enum PaymentMethod
    {   
        COD,
        OnlinePayment
    }
}
