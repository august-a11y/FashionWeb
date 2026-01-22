using FashionShop.Domain.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FashionShop.Application.Interfaces
{
    public interface IOrderService
    {
        Task<Guid> CreateOrderAsync(OrderInputDto orderInput, CancellationToken cancellation);
        Task<OrderPreviewDto> PreviewOderAsync(OrderInputDto orderInput, CancellationToken cancellationToken);
        Task<OrderDetailDto> GetOderByIdAsync(Guid orderId, CancellationToken cancellationToken);
        Task<List<OrderDetailDto>> GetOderByUserAync(Guid userId, CancellationToken cancellationToken);
        Task CancelOderAsync(Guid orderId);
        Task UpdateOderStatusAsync(Guid orderId, string status, CancellationToken cancellation);
    }
}
