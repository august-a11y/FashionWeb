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

namespace FashionShop.Application.Categories.Query
{
    public class GetCategoryByIdQuery : IRequest<IEnumerable<CategoryDto>>;


    public class GetCategoryByIdQueryHandler : IRequestHandler<GetCategoryByIdQuery, IEnumerable<CategoryDto>>
    {
        private readonly IRepository<Category, Guid > _repository;
        private readonly IMapper _mapper;

        public GetCategoryByIdQueryHandler(IRepository<Category, Guid> repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }
        public async Task<IEnumerable<CategoryDto>> Handle(GetCategoryByIdQuery request, CancellationToken cancellationToken)
        {
            var categories = await _repository.GetAllAsync(cancellationToken);
            var categoriesDto = _mapper.Map<IEnumerable<CategoryDto>>(categories);
            return _mapper.Map<IEnumerable<CategoryDto>>(categoriesDto);
        }
    }
}
