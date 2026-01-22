using FashionShop.Application.Categories.Commands;
using FashionShop.Application.Dtos;
using FashionShop.Application.Products.Commands;
using FashionShop.Domain.Entities;
using FashionShop.Domain.Interfaces;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FashionShop.Application.Categories
{
    public class CategoryValidation : AbstractValidator<CreateCategoryCommand>
    {
        public CategoryValidation(IRepository<Category, Guid> categoryRepository)
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
