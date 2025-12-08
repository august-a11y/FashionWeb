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
    public record GetCategoryQuery : IRequest<List<Category>>;
    


    public class GetCategoryQueryHandler : IRequestHandler<GetCategoryQuery, List<Category>>
    {
        private readonly ICategoryRepository _categoryRepository;
        public GetCategoryQueryHandler(ICategoryRepository categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }
        public async Task<List<Category>> Handle(GetCategoryQuery request, CancellationToken cancellationToken)
        {
            var categories = await _categoryRepository.GetCategories();
            return Task.FromResult(categories.ToList()).Result;
        }
    }


}
