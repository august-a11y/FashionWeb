using AutoMapper;
using AutoMapper.QueryableExtensions;
using FashionShop.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using FashionShop.Application.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FashionShop.Application.RequestDtos;
using FashionShop.Application.Common.Interfaces;

namespace FashionShop.Application.Products.Queries
{
    public class GetListProductQuery : IRequest<IEnumerable<ProductDto>>
    {
        public string? Keyword { get; set; }
        public Guid? CategoryId { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }

    
        public string? SortColumn { get; set; } 
        public string? SortOrder { get; set; }
    }

    public class GetListProductQueryHandler : IRequestHandler<GetListProductQuery, IEnumerable<ProductDto>>
    {
        
        private readonly IMapper _mapper;
        private readonly IApplicationDbContext _context;
        public GetListProductQueryHandler(IMapper mapper, IApplicationDbContext context)
        {
            _context = context;
            
            _mapper = mapper;
        }
        public  async Task<IEnumerable<ProductDto>> Handle(GetListProductQuery request, CancellationToken cancellationToken)
        {
            var query = _context.Products.AsNoTracking();

            
            query = query.FilterByName(request.Keyword)    
                         .FilterByCategory(request.CategoryId)
                         .FilterByPriceRange(request.MinPrice, request.MaxPrice);

            
            query = query.ApplySorting(request.SortColumn, request.SortOrder);

            
            return await query
                .ProjectTo<ProductDto>(_mapper.ConfigurationProvider)
                .ToListAsync(cancellationToken);

        }
    }
}
