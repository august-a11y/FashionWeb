using AutoMapper;
using FashionShop.Application.RequestDtos;
using FashionShop.Domain.Common;
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
    public record GetAllProductQuery : IRequest<IEnumerable<ProductDto>>;
    


    public class GetProductQueryHandler: IRequestHandler<GetAllProductQuery, IEnumerable<ProductDto>>
    {
        private readonly IRepository<Product, Guid> _repo ; 
        private readonly IMapper _mapper;
        public GetProductQueryHandler(IRepository<Product, Guid> repo, IMapper mapper)
        {
            _repo = repo;
            _mapper = mapper;
        }


        public async Task<IEnumerable<ProductDto>> Handle(GetAllProductQuery request, CancellationToken cancellationToken)
        {
            var Entity = await _repo.GetAllAsync(cancellationToken);
            var EntityDto = _mapper.Map<IEnumerable<ProductDto>>(Entity);
            return EntityDto;
        }
    }


}
