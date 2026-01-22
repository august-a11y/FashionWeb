using FashionShop.Application.Interfaces;
using FashionShop.Domain.Common.Interfaces;
using FashionShop.Domain.Dto;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FashionShop.Application.Cart.Commands
{
    public class RemoveItemFromCartCommand : IRequest<bool>
    {
        public Guid productId { get; set; }
        public Guid variantId { get; set; }
    }
    
    public class RemoveItemFromCartCommandHandler : IRequestHandler<RemoveItemFromCartCommand, bool>
    {
        private readonly ICartService _cartService;
        private readonly IInventoryService _inventoryService;
        public RemoveItemFromCartCommandHandler(ICartService cartService, IInventoryService inventoryService)
        {
            _inventoryService = inventoryService;
            _cartService = cartService;
        }
        public async Task<bool> Handle(RemoveItemFromCartCommand request, CancellationToken cancellationToken)
        {
            await _cartService.RemoveItemAsync(request.productId, request.variantId, cancellationToken);
            return true;
        }
    }
}
