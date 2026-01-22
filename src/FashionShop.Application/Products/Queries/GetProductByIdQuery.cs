using AutoMapper;
using FashionShop.Application.RequestDtos;
using FashionShop.Domain.Entities;
using FashionShop.Domain.Interfaces;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FashionShop.Application.Products.Queries
{
    public class GetProductByIdQuery() : IRequest<IEnumerable<ProductDto>>
    {
        public Guid Id { get; set; }
    }
    public class GetProductByIdHandler : IRequestHandler<GetProductByIdQuery, IEnumerable<ProductDto>>
    {
        private readonly IRepository<Product, Guid> _productRepo;
        private readonly IMapper _mapper;
        public GetProductByIdHandler(IRepository<Product, Guid> productRepo, IMapper mapper)
        {
            _productRepo = productRepo;
            _mapper = mapper;
        }
        public  Task<IEnumerable<ProductDto>> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
        {
            var product =  _productRepo.Find(p => p.Id == request.Id);
            var productDto = _mapper.Map<IEnumerable<ProductDto>>(product);
            return Task.FromResult(productDto);
        }
    }
}
