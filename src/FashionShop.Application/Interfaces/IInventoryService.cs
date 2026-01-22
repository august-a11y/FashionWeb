using FashionShop.Domain.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FashionShop.Application.Interfaces
{
    public interface IInventoryService
    {
        Task<(bool IsAvailable, string Message)> HasEnoughStockAsync(CartItemInputDto cartItem, CancellationToken cancellationToken);


        Task<(bool IsAvailable, string Message)> ReduceStockAsync(CartItemInputDto cartItem, CancellationToken cancellationToken);


        Task<(bool IsAvailable, string Message)> RestoreStockAsync(CartItemInputDto cartItem, CancellationToken cancellationToken);
    }
}
