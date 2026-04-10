using FashionShop.Application.Services.OrderServices.DTO;
using FashionShop.Domain.Entities;
using FluentResults;

namespace FashionShop.Application.Services.OrderServices
{
    public interface IOrderService
    {
        Task<Result<OrderDTO>> CreateOrderAsync(CreateOrderDTO createOrderDto, CancellationToken cancellationToken);
        Task<Result<OrderDTO>> GetOrderByIdAsync(Guid orderId, CancellationToken cancellationToken);
        Task<Result<List<OrderDTO>>> GetOrdersByUserIdAsync(Guid userId, CancellationToken cancellationToken);
        Task<Result<bool>> CancelOrderAsync(Guid orderId, CancellationToken cancellationToken);
        Task<Result<OrderPreviewDto>> PreviewOderAsync(CreateOrderDTO orderInput, CancellationToken cancellationToken);
        Task<Result<bool>> UpdateOderStatusAsync(Guid orderId, OrderStatus status, CancellationToken cancellationToken);
        Task<Result<List<OrderDTO>>> GetAllOrdersAsync(CancellationToken cancellationToken);

    }
}
