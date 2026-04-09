using FashionShop.Application.Services.ProductServices;
using FashionShop.Application.Services.ProductServices.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FashionShop.API.Controllers.Admin
{
    [ApiController]
    [Route("api/admin/products")]
    [Authorize(Policy = "Admin")]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _productService;

        public ProductsController(IProductService productService)
        {
            _productService = productService;
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse<ProductResponseDTO>>> CreateProduct([FromBody] CreateProductDTO createProductDTO, CancellationToken cancellationToken)
        {
            try
            {
                var result = await _productService.CreateProductAsync(createProductDTO, cancellationToken);
                if (result == null)
                    return StatusCode(500, ApiResponse.CreateFailureResponse("Failed to create product.", 500));

                return StatusCode(StatusCodes.Status201Created, ApiResponse<ProductResponseDTO>.CreateSuccessResponse(result.Value, "Product created successfully."));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse.CreateFailureResponse($"Failed to create product: {ex.Message}", 500));
            }
        }

        [HttpPut("{id:guid}")]
        public async Task<ActionResult<ApiResponse<ProductResponseDTO>>> UpdateProduct([FromRoute] Guid id, [FromBody] UpdateDetailsProductDTO updateDetailsProductDTO, CancellationToken cancellationToken)
        {
            if (id == Guid.Empty)
                return BadRequest(ApiResponse.CreateFailureResponse("Invalid product id.", 400));

            var result = await _productService.UpdateProductAsync(id, updateDetailsProductDTO, cancellationToken);
            if (result.IsFailed)
            {
                var message = result.Errors.FirstOrDefault()?.Message ?? "Failed to update product.";
                if (message.Contains("not found", StringComparison.OrdinalIgnoreCase))
                    return NotFound(ApiResponse.CreateFailureResponse(message, 404));

                return BadRequest(ApiResponse.CreateFailureResponse(message, 400));
            }

            return Ok(ApiResponse<ProductResponseDTO>.CreateSuccessResponse(result.Value, "Product updated successfully."));
        }

        [HttpDelete("{id:guid}")]
        public async Task<ActionResult<ApiResponse>> DeleteProduct([FromRoute] Guid id, CancellationToken cancellationToken)
        {
            if (id == Guid.Empty)
                return BadRequest(ApiResponse.CreateFailureResponse("Invalid product id.", 400));

            var result = await _productService.DeleteProductAsync(id, cancellationToken);
            if (result.IsFailed)
            {
                var message = result.Errors.FirstOrDefault()?.Message ?? "Failed to delete product.";
                if (message.Contains("not found", StringComparison.OrdinalIgnoreCase))
                    return NotFound(ApiResponse.CreateFailureResponse(message, 404));

                return BadRequest(ApiResponse.CreateFailureResponse(message, 400));
            }

            return Ok(ApiResponse.CreateSuccessResponse("Product deleted successfully."));
        }
    }
}
