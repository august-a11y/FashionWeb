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

namespace FashionShop.Application.Products.Validation
{
    public class ProductValidator : AbstractValidator<CreateProductCommand>
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
            RuleFor(p => p.CategoryID)
                .NotEmpty().WithMessage("Category ID must be a positive integer.");
            RuleFor(p => p.CategoryID)
                .MustAsync(async (command, categoryId, cancellation) =>
                {
                    var category = await categoryRepository.GetByIdAsync(categoryId, cancellation);
                    return category != null;
                }).WithMessage("Category ID does not exist.");
        }
    }
}
