using FashionShop.Application.Interfaces;

namespace FashionShop.Infrastructure.Services
{
    public class SendGridEmailService : IEmailService
    {
        public Task SendOrderConfirmationAsync(string email, string name, string orderId)
        {
            return Task.CompletedTask;
        }
    }
}
