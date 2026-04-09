using FashionShop.Application.Common.DTOs;
using FashionShop.Application.Interfaces;
using FashionShop.Application.Services.CategoryServices.DTO;
using FashionShop.Application.Services.ProductServices;
using FashionShop.Application.Services.ProductServices.DTO;
using FashionShop.Application.Specifications;
using FluentResults;
using System.Globalization;
using System.Text;

namespace FashionShop.Application.Services.CategoryServices
{
    public class CategoryService : ICategoryService
    {
        private readonly IRepository<Domain.Entities.Category> _categoryRepository;
        private readonly IProductService _productService;
        private readonly IUnitOfWork _unitOfWork;

        public CategoryService(
            IRepository<Domain.Entities.Category> categoryRepository,
            IProductService productService,
            IUnitOfWork unitOfWork)
        {
            _categoryRepository = categoryRepository;
            _productService = productService;
            _unitOfWork = unitOfWork;
        }
        public async Task<Result<CategoryDTO>> CreateCategoryAsync(CreateCategoryDTO createCategoryDto, CancellationToken cancellationToken)
        {
            try
            {
                var category = new Domain.Entities.Category(createCategoryDto.Name, createCategoryDto.Description);
                await _categoryRepository.AddAsync(category, cancellationToken);
                await _unitOfWork.CommitAsync(cancellationToken);
                return Result.Ok(new CategoryDTO
                {
                    Id = category.Id,
                    Name = category.Name,
                    Description = category.Description
                });
            }
            catch (Exception ex)
            {
                return Result.Fail<CategoryDTO>(ex.Message);
            }

        }

        public async Task<Result<bool>> DeleteCategoryAsync(Guid id, CancellationToken cancellationToken)
        {
            var category = await _categoryRepository.GetByIdAsync(id, cancellationToken);
            if (category == null)
            {
                return Result.Fail($"Category with id {id} not found.");
            }
            await _categoryRepository.DeleteAsync(category, cancellationToken);
            await _unitOfWork.CommitAsync(cancellationToken);
            return Result.Ok(true);
        }

        public async Task<Result<List<CategoryDTO>>> GetAllCategoriesAsync(CancellationToken cancellationToken)
        {
            var result = await _categoryRepository.ListAsync(cancellationToken);
            var categories = result.Select(c => new CategoryDTO
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description
            }).ToList();
            return Result.Ok(categories);

        }

        public async Task<Result<CategoryDTO>> GetCategoryByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            var category = await _categoryRepository.GetByIdAsync(id, cancellationToken);
            if (category == null)
            {
                return Result.Fail($"Category with id {id} not found.");
            }
            return Result.Ok(new CategoryDTO
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description
            });
        }

        public async Task<Result<PageResponse<ProductResponseDTO>>> GetProductsByCategoryIdAsync(
            Guid categoryId,
            int pageIndex,
            int pageSize,
            CancellationToken cancellationToken)
        {




            var parentCategory = await _categoryRepository.GetByIdAsync(categoryId, cancellationToken);
            if (parentCategory == null)
            {
                return Result.Fail<PageResponse<ProductResponseDTO>>($"Category not found.");
            }
            var spec = new CategoryFilterSpecification(parentCategoryId: categoryId);
            var root = await _categoryRepository.ListAsync(spec, cancellationToken);
            
            if (root == null)
                return await _productService.GetProductsByCategoryAsync(pageIndex, pageSize, categoryId, cancellationToken);
            var subCategoryIds = root.Select(c => c.Id).ToList();
            return await _productService.GetProductsByListCategoryIdAsync(subCategoryIds, pageIndex, pageSize, cancellationToken);
        }



        public async Task<Result<CategoryDTO>> UpdateCategoryAsync(Guid categoryId, UpdateCategoryDTO updateCategoryDto, CancellationToken cancellationToken)
        {
            var category = await _categoryRepository.GetByIdAsync(categoryId, cancellationToken);
            if (category == null)
            {
                return Result.Fail($"Category with id {categoryId} not found.");
            }
            category.UpdateDetails(updateCategoryDto.Name, updateCategoryDto.Description);
            await _unitOfWork.CommitAsync(cancellationToken);
            return Result.Ok(new CategoryDTO
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description
            });

        }
    }
}
