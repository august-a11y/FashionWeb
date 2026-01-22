using FashionShop.Application.Interfaces;
using FashionShop.Domain.Dto;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FashionShop.Application.Order.Commands
{
    public class CreateOrderCommand : IRequest<Guid>
    {
        public List<OrderItemDto> OrderItems { get; set; } = new List<OrderItemDto>();
        public AddressDto address { get; set; } = new AddressDto();
        public string PhoneNumber { get; set; } = string.Empty;
        public string Note { get; set; } = string.Empty;
        public PaymentMethod PaymentMethod { get; set; }
    }

    public class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, Guid>
    {
        private readonly IOrderService _orderService;
        public CreateOrderCommandHandler(IOrderService orderService)
        {
            _orderService = orderService;
        }
        public async Task<Guid> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
        {
            var orderInput = new OrderInputDto
            {
                OrderItems = request.OrderItems,
                ShippingAddress = request.address,
                PhoneNumber = request.PhoneNumber,
                Note = request.Note,
                PaymentMethod = request.PaymentMethod
            };
            var orderId = await _orderService.CreateOrderAsync(orderInput, cancellationToken);
            return orderId;
        }
    }

}
