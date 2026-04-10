using FashionShop.Application.Common.DTOs;
using FashionShop.Application.Services.CategoryServices;
using FashionShop.Application.Services.CategoryServices.DTO;
using FashionShop.Application.Services.ProductServices.DTO;
using Microsoft.AspNetCore.Mvc;

namespace FashionShop.API.Controllers.User
{
    [ApiController]
    [Route("api/categories")]
    public class CategoryController : ControllerBase
    {
        private readonly ICategoryService _categoryService;

        public CategoryController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<List<CategoryDTO>>>> GetAllCategories(CancellationToken cancellationToken)
        {
            var result = await _categoryService.GetAllCategoriesAsync(cancellationToken);
            if (result.IsFailed)
            {
                var message = result.Errors.FirstOrDefault()?.Message ?? "Failed to retrieve categories.";
                return BadRequest(ApiResponse.CreateFailureResponse(message, 400));
            }

            return Ok(ApiResponse<List<CategoryDTO>>.CreateSuccessResponse(result.Value, "Categories retrieved successfully."));
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<ApiResponse<CategoryDTO>>> GetCategoryById([FromRoute] Guid id, CancellationToken cancellationToken)
        {
            if (id == Guid.Empty)
                return BadRequest(ApiResponse.CreateFailureResponse("Invalid category id.", 400));

            var result = await _categoryService.GetCategoryByIdAsync(id, cancellationToken);
            if (result.IsFailed)
            {
                var message = result.Errors.FirstOrDefault()?.Message ?? "Category not found.";
                return NotFound(ApiResponse.CreateFailureResponse(message, 404));
            }

            return Ok(ApiResponse<CategoryDTO>.CreateSuccessResponse(result.Value, "Category retrieved successfully."));
        }

        [HttpGet("{categoryId}/products")]
        public async Task<ActionResult<ApiResponse<PageResponse<ProductResponseDTO>>>> GetProductsByCategoryName(
            [FromRoute] Guid categoryId,
            [FromQuery] int pageIndex = 1,
            [FromQuery] int pageSize = 10,
            CancellationToken cancellationToken = default)
        {
            if (pageIndex < 1 || pageSize < 1)
            {
                return BadRequest(ApiResponse.CreateFailureResponse("pageIndex and pageSize must be greater than 0.", 400));
            }

            var result = await _categoryService.GetProductsByCategoryIdAsync(categoryId, pageIndex, pageSize, cancellationToken);
            if (result.IsFailed)
            {
                var message = result.Errors.FirstOrDefault()?.Message ?? "Failed to retrieve products.";
                if (message.Contains("not found", StringComparison.OrdinalIgnoreCase))
                {
                    return NotFound(ApiResponse.CreateFailureResponse(message, 404));
                }

                return BadRequest(ApiResponse.CreateFailureResponse(message, 400));
            }

            return Ok(ApiResponse<PageResponse<ProductResponseDTO>>.CreateSuccessResponse(result.Value, "Products retrieved successfully."));
        }

        [HttpGet("parents")]
        public async Task<ActionResult<ApiResponse<List<CategoryDTO>>>> GetParentCategories(CancellationToken cancellationToken)
        {
            var result = await _categoryService.GetParentCategoriesAsync(cancellationToken);
            if (result.IsFailed)
            {
                var message = result.Errors.FirstOrDefault()?.Message ?? "Failed to retrieve parent categories.";
                return BadRequest(ApiResponse.CreateFailureResponse(message, 400));
            }

            return Ok(ApiResponse<List<CategoryDTO>>.CreateSuccessResponse(result.Value, "Parent categories retrieved successfully."));
        }

        [HttpGet("{parentId:guid}/sub")]
        public async Task<ActionResult<ApiResponse<List<CategoryDTO>>>> GetSubCategoriesByParentId(
            [FromRoute] Guid parentId,
            CancellationToken cancellationToken)
        {
            if (parentId == Guid.Empty)
                return BadRequest(ApiResponse.CreateFailureResponse("Invalid parent category id.", 400));

            var result = await _categoryService.GetSubCategoriesByParentIdAsync(parentId, cancellationToken);
            if (result.IsFailed)
            {
                var message = result.Errors.FirstOrDefault()?.Message ?? "Failed to retrieve child categories.";
                if (message.Contains("not found", StringComparison.OrdinalIgnoreCase))
                {
                    return NotFound(ApiResponse.CreateFailureResponse(message, 404));
                }

                return BadRequest(ApiResponse.CreateFailureResponse(message, 400));
            }

            return Ok(ApiResponse<List<CategoryDTO>>.CreateSuccessResponse(result.Value, "Child categories retrieved successfully."));
        }

    }
}
