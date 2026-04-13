using FashionShop.Application.Interfaces;
using FashionShop.Contracts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace FashionShop.Infrastructure.Services
{
    public class SendGridEmailService : IEmailService
    {
        private readonly ILogger<SendGridEmailService> _logger;
        private readonly string _apiKey;
        private readonly string _fromEmail;
        private readonly string _fromName;

        public SendGridEmailService(
            IConfiguration configuration,
            ILogger<SendGridEmailService> logger)
        {
            _logger = logger;
            _apiKey = configuration["SendGrid:ApiKey"] ?? string.Empty;
            _fromEmail = configuration["SendGrid:FromEmail"] ?? "no-reply@fashionshop.local";
            _fromName = configuration["SendGrid:FromName"] ?? "FashionShop";
        }

        public async Task SendOrderConfirmationAsync(OrderCreatedEvent orderCreatedEvent)
        {
            if (string.IsNullOrWhiteSpace(_apiKey))
            {
                _logger.LogWarning("SendGrid API key is missing. Skip sending email for order {OrderCode}.", orderCreatedEvent.OrderCode);
                return;
            }

            if (string.IsNullOrWhiteSpace(orderCreatedEvent.CustomerEmail))
            {
                _logger.LogWarning("Customer email is empty. Skip sending email for order {OrderCode}.", orderCreatedEvent.OrderCode);
                return;
            }

            var client = new SendGridClient(_apiKey);

            var from = new EmailAddress(_fromEmail, _fromName);
            var to = new EmailAddress(orderCreatedEvent.CustomerEmail, orderCreatedEvent.CustomerName);

            var subject = $"[FashionShop] Order confirmation #{orderCreatedEvent.OrderCode}";
            var plainText = $"""
                Xin Chào {orderCreatedEvent.CustomerName},

                Đơn hàng của bạn đã được đặt thành công.

                Order code: {orderCreatedEvent.OrderCode}
                Trạng thái: {orderCreatedEvent.OrderStatus}
                Tổng tiền: {orderCreatedEvent.TotalAmount:N0}
                Số điện thoai: {orderCreatedEvent.CustomerPhone}
                Địa chỉ giao hàng: {orderCreatedEvent.ShippingAddress}
                Đặt vào lúc: {orderCreatedEvent.CreatedAt:yyyy-MM-dd HH:mm:ss}

                Cảm ơn bạn mua sắm từ hệ thống của chúng tôi.
                LVShop Cảm ơn.
                """;

            var html = $"""
                <h3>Xin Chào {orderCreatedEvent.CustomerName},</h3>
                <p>Đơn hàng của bạn đã được đặt thành công.</p>
                <ul>
                    <li><b>Order code:</b> {orderCreatedEvent.OrderCode}</li>
                    <li><b>Trạng thái:</b> {orderCreatedEvent.OrderStatus}</li>
                    <li><b>Tổng tiền:</b> {orderCreatedEvent.TotalAmount:N0}</li>
                    <li><b>Số điện thoại:</b> {orderCreatedEvent.CustomerPhone}</li>
                    <li><b>Địa chỉ giao hàng:</b> {orderCreatedEvent.ShippingAddress}</li>
                    <li><b>Đặt vào lúc:</b> {orderCreatedEvent.CreatedAt:yyyy-MM-dd HH:mm:ss}</li>
                </ul>
                <p>Thank you for shopping with FashionShop.</p>
                """;

            var message = MailHelper.CreateSingleEmail(from, to, subject, plainText, html);
            var response = await client.SendEmailAsync(message);

            if ((int)response.StatusCode >= 400)
            {
                var body = await response.Body.ReadAsStringAsync();
                _logger.LogError($"SendGrid failed for order {orderCreatedEvent.OrderCode}.",
                    orderCreatedEvent.OrderCode, response.StatusCode, body);
                return;
            }

            _logger.LogInformation("Order confirmation email sent successfully for order {OrderCode}.", orderCreatedEvent.OrderCode);
        }
    }
}
