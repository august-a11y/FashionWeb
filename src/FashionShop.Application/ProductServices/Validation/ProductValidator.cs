using FashionShop.Application.ProductServices.DTO;
using FashionShop.Domain.Entities;
using FashionShop.Domain.Interfaces;
using FluentValidation;

namespace FashionShop.Application.ProductServices.Validation
{
    public class ProductValidator : AbstractValidator<CreateProductDTO>
    {
        public ProductValidator(IRepository<Category, Guid> categoryRepository)
        {
            RuleFor(p => p.Name)
                .NotEmpty().WithMessage("Product name is required.")
                .MaximumLength(100).WithMessage("Product name must not exceed 100 characters.");
            RuleFor(p => p.Description)
                .MaximumLength(500).WithMessage("Description must not exceed 500 characters.");
            RuleFor(p => p.BasePrice)
                .GreaterThan(0).WithMessage("Price must be greater than zero.");
            RuleFor(p => p.CategoryId)
                .NotEmpty().WithMessage("Category ID must be a positive integer.");
            RuleFor(p => p.CategoryId)
                .MustAsync(async (command, categoryId, cancellation) =>
                {
                    var category = await categoryRepository.GetByIdAsync(categoryId, cancellation);
                    return category != null;
                }).WithMessage("Category ID does not exist.");
        }
    }
}
