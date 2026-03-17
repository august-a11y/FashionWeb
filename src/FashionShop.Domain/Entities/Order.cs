using FashionShop.Domain.Common;
using FashionShop.Domain.Identity;



namespace FashionShop.Domain.Entities
{
    public class Order : BaseEntity
    {
        public string OrderCode { get; set; }
        public Guid UserId { get; set; }
        public virtual AppUser User { get; set; } 

        public OrderAddressSnapshot ShippingAddress { get; set; } 
        public string PhoneNumber { get; set; } = string.Empty;
        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
        public decimal SubTotal { get; set; }
        public decimal ShippingFee { get; set; }
        public decimal TotalAmount { get; set; }
        public string Note { get; set; } = string.Empty;
        public virtual Payment Payment { get; set; } 
        public virtual Shipment Shipment { get; set; } 
        public OrderStatus Status { get; set; }
        public DateTime? ExpectedDeliveryDate { get; set; }
        public byte[] RowVersion { get; set; } = Array.Empty<byte>();
    }

    public class OrderItem : BaseEntity
    {
        public Guid OrderId { get; set; }
        public Guid ProductId { get; set; }
        public Guid VariantId { get; set; }
        public string ProductName { get; set; } 
        public string Color { get; set; }
        public string Size { get; set; } 
        public string ThumbnailUrl { get; set; } = string.Empty;

        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal LineTotal => UnitPrice * Quantity;

        public virtual Order Order { get; set; } = null!;
    }

    public enum PaymentMethod { COD, CreditCard, Momo, PayPal }
    public enum PaymentStatus { Pending, Success, Failed, Refunded }
    public enum ShipmentStatus { Preparing, InTransit, Delivered, Returned }
    public enum OrderStatus { Pending, Processing, Shipped, Delivered, Cancelled }

    public class Payment : BaseEntity
    {
        public Guid OrderId { get; set; }
        public virtual Order Order { get; set; } = null!;
        public PaymentMethod PaymentMethod { get; set; }
        public decimal Amount { get; set; }
        public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
        public string TransactionId { get; set; } = string.Empty;
    }

    public class Shipment : BaseEntity
    {
        public Guid OrderId { get; set; }
        public virtual Order Order { get; set; } = null!;
        public string Carrier { get; set; } = string.Empty;
        public string TrackingNumber { get; set; } = string.Empty;
        public ShipmentStatus Status { get; set; } = ShipmentStatus.Preparing;
        public DateTime? EstimatedDeliveryDate { get; set; }
        public DateTime? ShippedAt { get; set; }
    }

    public class OrderAddressSnapshot
    {
        public string RecipientName { get; set; } = string.Empty;
        public string RecipientPhone { get; set; } = string.Empty;
        public string StreetAddress { get; set; } = string.Empty;
        public string Ward { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string District { get; set; } = string.Empty;
    }
}
