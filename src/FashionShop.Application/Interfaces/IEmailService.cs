using FashionShop.Contracts;

namespace FashionShop.Application.Interfaces
{
    public interface IEmailService
    {
        Task SendOrderConfirmationAsync(OrderCreatedEvent orderCreatedEvent);
    }
}
