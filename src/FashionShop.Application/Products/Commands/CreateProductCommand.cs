using FashionShop.Domain.Entities;
using FashionShop.Domain.Interfaces;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FashionShop.Application.Products.Commands
{
    public record CreateProductCommand : IRequest<int>
    {
        public string Name { get; init; } = string.Empty;
        public string Description { get; init; } = string.Empty;
        public decimal Price { get; init; }
        public string ImageUrl { get; init; } = string.Empty;
        public string Size { get; init; } = string.Empty;
        public string Color { get; init; } = string.Empty;
        public int StockQuantity { get; init; }
        public int CategoryId { get; init; }
    }

    public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, int>
    {
        private readonly IProductRepository _productRepository;
        public CreateProductCommandHandler(IProductRepository productRepository)
        {
            _productRepository = productRepository;
        }
        public async Task<int> Handle(CreateProductCommand request, CancellationToken cancellationToken)
        {
            var product = new Product
            {
                Name = request.Name,
                Description = request.Description,
                Price = request.Price,
                ImageUrl = request.ImageUrl,
                Size = request.Size,
                Color = request.Color,
                StockQuantity = request.StockQuantity,
                CategoryId = 1

            };
            return await _productRepository.AddAsync(product);
        }
    }
}
