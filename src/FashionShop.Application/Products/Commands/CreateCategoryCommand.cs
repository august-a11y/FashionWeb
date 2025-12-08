using FashionShop.Domain.Entities;
using FashionShop.Domain.Interfaces;
using MediatR;


namespace FashionShop.Application.Products.Commands
{
    public record CreateCategoryCommand : IRequest<int>
    {
        public string Name { get; init; } = string.Empty;
        public ICollection<Product> Products { get; init; } = new List<Product>();
    }
    

    public class CreateCategoryCommandHandler : IRequestHandler<CreateCategoryCommand, int>
    {
        private readonly ICategoryRepository _categoryRepository;
        public CreateCategoryCommandHandler(ICategoryRepository categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }
        public async Task<int> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
        {
            var category = new Category
            {
                Name = request.Name,
                Products = request.Products
            };
            return await _categoryRepository.CreateCategory(category);
        }
    }
}
