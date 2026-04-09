using FashionShop.Application.Services.CategoryServices;
using FashionShop.Application.Services.CategoryServices.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FashionShop.API.Controllers.Admin
{
    [ApiController]
    [Route("api/admin/categories")]
    [Authorize(Policy = "Admin")]
    public class CategoryController : ControllerBase
    {
        private readonly ICategoryService _categoryService;

        public CategoryController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse<CategoryDTO>>> CreateCategory([FromBody] CreateCategoryDTO createCategoryDTO, CancellationToken cancellationToken)
        {
            var result = await _categoryService.CreateCategoryAsync(createCategoryDTO, cancellationToken);
            if (result.IsFailed)
            {
                var message = result.Errors.FirstOrDefault()?.Message ?? "Failed to create category.";
                return BadRequest(ApiResponse.CreateFailureResponse(message, 400));
            }

            return StatusCode(StatusCodes.Status201Created, ApiResponse<CategoryDTO>.CreateSuccessResponse(result.Value, "Category created successfully."));
        }

        [HttpPut("{id:guid}")]
        public async Task<ActionResult<ApiResponse<CategoryDTO>>> UpdateCategory([FromRoute] Guid id, [FromBody] UpdateCategoryDTO updateCategoryDTO, CancellationToken cancellationToken)
        {
            if (id == Guid.Empty)
                return BadRequest(ApiResponse.CreateFailureResponse("Invalid category id.", 400));

            var result = await _categoryService.UpdateCategoryAsync(id, updateCategoryDTO, cancellationToken);
            if (result.IsFailed)
            {
                var message = result.Errors.FirstOrDefault()?.Message ?? "Failed to update category.";
                if (message.Contains("not found", StringComparison.OrdinalIgnoreCase))
                    return NotFound(ApiResponse.CreateFailureResponse(message, 404));

                return BadRequest(ApiResponse.CreateFailureResponse(message, 400));
            }

            return Ok(ApiResponse<CategoryDTO>.CreateSuccessResponse(result.Value, "Category updated successfully."));
        }

        [HttpDelete("{id:guid}")]
        public async Task<ActionResult<ApiResponse>> DeleteCategory([FromRoute] Guid id, CancellationToken cancellationToken)
        {
            if (id == Guid.Empty)
                return BadRequest(ApiResponse.CreateFailureResponse("Invalid category id.", 400));

            var result = await _categoryService.DeleteCategoryAsync(id, cancellationToken);
            if (result.IsFailed)
            {
                var message = result.Errors.FirstOrDefault()?.Message ?? "Failed to delete category.";
                if (message.Contains("not found", StringComparison.OrdinalIgnoreCase))
                    return NotFound(ApiResponse.CreateFailureResponse(message, 404));

                return BadRequest(ApiResponse.CreateFailureResponse(message, 400));
            }

            return Ok(ApiResponse.CreateSuccessResponse("Category deleted successfully."));
        }
    }
}
