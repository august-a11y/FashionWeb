using FashionShop.Domain.Common;
using FashionShop.Domain.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FashionShop.Domain.Entities
{
    public class Cart : BaseEntity
    {
        
        public Guid UserId { get; set; }
        public DateTime LastUpdate { get; set; } = DateTime.UtcNow;
        public virtual ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
    }
}
