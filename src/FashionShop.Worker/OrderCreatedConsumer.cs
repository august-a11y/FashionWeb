using FashionShop.Application.Interfaces;
using FashionShop.Contracts;
using MassTransit;

public class OrderCreatedConsumer : IConsumer<OrderCreatedEvent>
{
    private readonly IEmailService _emailService;

    public OrderCreatedConsumer(IEmailService emailService)
    {
        _emailService = emailService;
    }

    public async Task Consume(ConsumeContext<OrderCreatedEvent> context)
    {
        var message = context.Message;

        await _emailService.SendOrderConfirmationAsync(message);
    }
}