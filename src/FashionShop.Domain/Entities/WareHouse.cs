

using FashionShop.Domain.Common;

namespace FashionShop.Domain.Entities
{
    public class Warehouse : BaseEntity
    {
        public string Name { get; set; } = string.Empty;  
        public Address Address { get; set; } = null!;   
    }
}