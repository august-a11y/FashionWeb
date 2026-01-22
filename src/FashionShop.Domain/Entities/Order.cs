using FashionShop.Domain.Common;


namespace FashionShop.Domain.Entities
{
    public class Order : BaseEntity
    {
        
        public Guid? UserId { get; set; }
     
        public Address ShippingAddress { get; set; } = null!;
        public string PhoneNumber { get; set; } = string.Empty;
        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
        public decimal SubTotal { get; set; }
        public decimal ShippingFee { get; set; }
        public decimal TotalAmount { get; set; }
        public string Note { get; set; } = string.Empty;
        public OrderStatus Status { get; set; }
        // Concurrency token
        public byte[] RowVersion { get; set; } = Array.Empty<byte>();
    }

    public class OrderItem : BaseEntity
    {
        
        public Guid OrderId {get; set; }

        public Guid ProductId { get; set; }
        public Guid ProductVariantId { get; set; }

        public string ProductName { get; set; } = string.Empty;
        public string Sku { get; set; } = string.Empty;

        public string Color { get; set; } = string.Empty;
        public string Size { get; set; } = string.Empty;

        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }

        public virtual Order Order { get; set; } = null!;
    }

    public enum OrderStatus { Pending, Paid, Shipped, Cancelled }
    
}
