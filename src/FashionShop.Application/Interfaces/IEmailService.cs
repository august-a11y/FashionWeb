namespace FashionShop.Application.Interfaces
{
    public interface IEmailService
    {
        Task SendOrderConfirmationAsync(string email, string name, string orderId);
    }
}
