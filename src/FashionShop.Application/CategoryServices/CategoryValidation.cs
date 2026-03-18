using FashionShop.Application.CategoryServices.DTO;
using FashionShop.Domain.Interfaces;
using FluentValidation;

namespace FashionShop.Application.CategoryServices
{
    public class CategoryValidation : AbstractValidator<CreateCategoryDTO>
    {
        public CategoryValidation(IRepository<Domain.Entities.Category, Guid> categoryRepository)
        {
            RuleFor(c => c.Name)
                .NotEmpty().WithMessage("Category name is required.")
                .MaximumLength(100).WithMessage("Category name must not exceed 100 characters.");
            RuleFor(c => c.Name)
                .MustAsync(async (command, name, cancellation) =>
                {
                    var existingCategories = await categoryRepository.GetAllAsync(cancellation);
                    return !existingCategories.Any(cat => cat.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
                }).WithMessage("Category name must be unique.");
        }
    }
}
