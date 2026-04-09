using FashionShop.Application.Common.DTOs;
using FashionShop.Application.Services.CategoryServices.DTO;
using FashionShop.Application.Services.ProductServices.DTO;
using FluentResults;

namespace FashionShop.Application.Services.CategoryServices
{
    public interface ICategoryService
    {
        Task<Result<List<CategoryDTO>>> GetAllCategoriesAsync(CancellationToken cancellationToken);
        Task<Result<CategoryDTO>> GetCategoryByIdAsync(Guid id, CancellationToken cancellationToken);
        Task<Result<PageResponse<ProductResponseDTO>>> GetProductsByCategoryIdAsync(Guid categoryId, int pageIndex, int pageSize, CancellationToken cancellationToken);
        Task<Result<CategoryDTO>> CreateCategoryAsync(CreateCategoryDTO createCategoryDto, CancellationToken cancellationToken);
        Task<Result<CategoryDTO>> UpdateCategoryAsync(Guid id, UpdateCategoryDTO updateCategoryDto, CancellationToken cancellationToken);
        Task<Result<bool>> DeleteCategoryAsync(Guid id, CancellationToken cancellationToken);
    }
}
