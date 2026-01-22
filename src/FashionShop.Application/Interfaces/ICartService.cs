using FashionShop.Domain.Common.Interfaces;
using FashionShop.Domain.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace FashionShop.Application.Interfaces
{
    public interface ICartService
    {
        Task AddToCartAsync(CartItemInputDto cartItem, CancellationToken cancellationToken);
        Task<CartOutputDto> GetCartAsync(CancellationToken cancellationToken);
        Task RemoveItemAsync(Guid productId, Guid variantId, CancellationToken cancellationToken);
        Task MergeCartsAsync(string anonymousCartId, CancellationToken cancellationToken);
        Task ReduceQuantityOfItemAsync(CartItemInputDto cartItem, CancellationToken cancellationToken);
    }
}
