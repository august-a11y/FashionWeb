using FashionShop.Application.Interfaces;
using FashionShop.Domain.Common.Interfaces;
using FashionShop.Domain.Dto;
using FashionShop.Domain.Entities;
using FashionShop.Domain.Interfaces;
using MediatR;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FashionShop.Application.Cart.Commands
{
    public class AddItemToCartCommand : IRequest<bool>
    {
        public Guid ProductId { get; set; }
        public Guid VariantId { get; set; }
        public int Quantity { get; set; }
    }

    public class AddItemToCartCommandHandler : IRequestHandler<AddItemToCartCommand, bool>
    {
        private readonly IInventoryService _inventoryService;
        private readonly ICartService _cartService;
        public AddItemToCartCommandHandler(IRepository<Product, Guid> productRepository, ICartService cartService, IInventoryService inventoryService)
        {
             _cartService = cartService;
            _inventoryService = inventoryService;
        }
        public async Task<bool> Handle(AddItemToCartCommand request, CancellationToken cancellationToken)
        {
            var cartItemInput = new CartItemInputDto
            {
                ProductId = request.ProductId,
                VariantId = request.VariantId,
                Quantity = request.Quantity
            };
 
 
            await _cartService.AddToCartAsync(cartItemInput, cancellationToken);
            
           
            return true;
        }
    }
}
