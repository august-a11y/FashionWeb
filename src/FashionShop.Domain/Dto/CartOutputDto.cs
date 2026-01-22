using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FashionShop.Domain.Dto
{
    public class CartOutputDto
    {
        public Guid UserId { get; set; }
        public int TotalItems { get; set; }
        public decimal TotalPrice { get; set; }
        public List<CartItemDto> Items { get; set; } = new List<CartItemDto>();
    }
}
