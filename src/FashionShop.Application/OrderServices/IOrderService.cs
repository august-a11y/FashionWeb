using FashionShop.Application.OrderServices.DTO;
using FluentResults;

namespace FashionShop.Application.OrderServices
{
    public interface IOrderService
    {
        Task<Result<OrderDTO>> CreateOrderAsync(CreateOrderDTO createOrderDto, CancellationToken cancellationToken);
        Task<Result<OrderDTO>> GetOrderByIdAsync(Guid orderId, CancellationToken cancellationToken);
        Task<Result<List<OrderDTO>>> GetOrdersByUserIdAsync(Guid userId, CancellationToken cancellationToken);
        Task<Result<bool>> CancelOrderAsync(Guid orderId, CancellationToken cancellationToken);
        Task<Result<OrderPreviewDto>> PreviewOderAsync(CreateOrderDTO orderInput, CancellationToken cancellationToken);
        Task<Result<bool>> UpdateOderStatusAsync(Guid orderId, string status, CancellationToken cancellationToken);


    }
}
