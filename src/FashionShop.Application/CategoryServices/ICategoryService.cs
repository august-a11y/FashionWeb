using FashionShop.Application.CategoryServices.DTO;
using FluentResults;

namespace FashionShop.Application.CategoryServices
{
    public interface ICategoryService
    {
        Task<Result<List<CategoryDTO>>> GetAllCategoriesAsync(CancellationToken cancellationToken);
        Task<Result<CategoryDTO>> GetCategoryByIdAsync(Guid id, CancellationToken cancellationToken);
        Task<Result<CategoryDTO>> CreateCategoryAsync(CreateCategoryDTO createCategoryDto, CancellationToken cancellationToken);
        Task<Result<CategoryDTO>> UpdateCategoryAsync(Guid id, UpdateCategoryDTO updateCategoryDto, CancellationToken cancellationToken);
        Task<Result<bool>> DeleteCategoryAsync(Guid id, CancellationToken cancellationToken);
    }
}
