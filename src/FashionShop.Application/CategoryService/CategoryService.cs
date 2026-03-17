using FashionShop.Application.CategoryService.DTO;
using FashionShop.Domain.Interfaces;
using FluentResults;

namespace FashionShop.Application.CategoryService
{
    public class CategoryService : ICategoryService
    {
        private readonly IRepository<Domain.Entities.Category, Guid> _categoryRepository;
        private readonly IUnitOfWork _unitOfWork;
        public CategoryService(IRepository<Domain.Entities.Category, Guid> categoryRepository, IUnitOfWork unitOfWork)
        {
            _categoryRepository = categoryRepository;
            _unitOfWork = unitOfWork;
        }
        public async Task<Result<CategoryDTO>> CreateCategoryAsync(CreateCategoryDTO createCategoryDto, CancellationToken cancellationToken)
        {
            try
            {
                var category = new Domain.Entities.Category(createCategoryDto.Name, createCategoryDto.Description);
                _categoryRepository.Add(category);
                await _unitOfWork.CommitAsync(cancellationToken);
                return Result.Ok(new CategoryDTO
                {
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
            if(category == null)
            {
                return Result.Fail($"Category with id {id} not found.");
            }
            _categoryRepository.Remove(category);
            await _unitOfWork.CommitAsync(cancellationToken);
            return Result.Ok(true);
        }

        public async Task<Result<List<CategoryDTO>>> GetAllCategoriesAsync(CancellationToken cancellationToken)
        {
            var result = await _categoryRepository.GetAllAsync(cancellationToken);
            var categories = result.Select(c => new CategoryDTO
            {
                Name = c.Name,
                Description = c.Description
            }).ToList();
            return Result.Ok(categories);

        }

        public async Task<Result<CategoryDTO>> GetCategoryByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            var category = await _categoryRepository.GetByIdAsync(id, cancellationToken);
            if(category == null)
            {
                return Result.Fail($"Category with id {id} not found.");
            }
            return Result.Ok(new CategoryDTO
            {
                Name = category.Name,
                Description = category.Description
            });
        }

        public async Task<Result<CategoryDTO>> UpdateCategoryAsync(Guid categoryId, UpdateCategoryDTO updateCategoryDto, CancellationToken cancellationToken)
        {
            var category = await _categoryRepository.GetByIdAsync(categoryId, cancellationToken);
            if(category == null)
            {
                return Result.Fail($"Category with id {categoryId} not found.");
            }
            category.UpdateDetails(updateCategoryDto.Name, updateCategoryDto.Description);
            await _unitOfWork.CommitAsync(cancellationToken);
            return Result.Ok(new CategoryDTO
            {
                Name = category.Name,
                Description = category.Description
            });

        }
    }
}
