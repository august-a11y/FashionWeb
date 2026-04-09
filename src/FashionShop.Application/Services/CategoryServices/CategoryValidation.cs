using FashionShop.Application.Interfaces;
using FashionShop.Application.Services.CategoryServices.DTO;
using FluentValidation;

namespace FashionShop.Application.Services.CategoryServices
{
    public class CategoryValidation : AbstractValidator<CreateCategoryDTO>
    {
        public CategoryValidation(IRepository<Domain.Entities.Category> categoryRepository)
        {
            RuleFor(c => c.Name)
                .NotEmpty().WithMessage("Category name is required.")
                .MaximumLength(100).WithMessage("Category name must not exceed 100 characters.");
            RuleFor(c => c.Name)
                .MustAsync(async (command, name, cancellation) =>
                {
                    var existingCategories = await categoryRepository.ListAsync(cancellation);
                    return !existingCategories.Any(cat => cat.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
                }).WithMessage("Category name must be unique.");
        }
    }
}
