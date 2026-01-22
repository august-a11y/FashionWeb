using FashionShop.Application.Interfaces;
using FashionShop.Domain.Common.Interfaces;
using FashionShop.Domain.Dto;
using MediatR;
using Microsoft.EntityFrameworkCore.Query;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace FashionShop.Application.Cart.Queries
{
    public class GetCartQuery : IRequest<CartOutputDto>
    {
    }

    public class GetCartQueryHandler : IRequestHandler<GetCartQuery, CartOutputDto>
    {
        private readonly ICartService _cartService;
        
        public GetCartQueryHandler(ICartService cartService)
        {
            _cartService = cartService;

        }
        public async Task<CartOutputDto> Handle(GetCartQuery request, CancellationToken cancellationToken)
        {
            var cart = await _cartService.GetCartAsync(cancellationToken);
            return cart;
        }
    }
}
