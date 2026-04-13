namespace FashionShop.Contracts
{
    public record OrderCreatedEvent(string OrderCode, string CustomerEmail, string CustomerName, string CustomerPhone, string ShippingAddress, string OrderStatus, decimal TotalAmount, DateTime CreatedAt );
}
