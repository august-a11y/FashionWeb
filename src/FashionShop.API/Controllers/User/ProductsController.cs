using FashionShop.Application.Common.DTOs;
using FashionShop.Application.Services.ProductServices;
using FashionShop.Application.Services.ProductServices.DTO;
using Microsoft.AspNetCore.Mvc;

namespace FashionShop.API.Controllers.User
{
    [ApiController]
    [Route("api/products")]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _productService;

        public ProductsController(IProductService productService)
        {
            _productService = productService;
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<PageResponse<ProductResponseDTO>>>> GetProducts([FromQuery]int pageIndex = 1, [FromQuery] int pageSize = 10, CancellationToken cancellationToken = default)
        {
            var result = await _productService.GetAllProductAsync(pageIndex, pageSize, cancellationToken);
            if (result.IsFailed)
            {
                var message = result.Errors.FirstOrDefault()?.Message ?? "Failed to retrieve products.";
                return StatusCode(500, ApiResponse.CreateFailureResponse(message, 500));
            }

            return Ok(ApiResponse<PageResponse<ProductResponseDTO>>.CreateSuccessResponse(result.Value, "Get Products Successfully"));
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<ApiResponse<ProductResponseDTO>>> GetProductById([FromRoute] Guid id, CancellationToken cancellationToken)
        {
            if (id == Guid.Empty)
                return BadRequest(ApiResponse.CreateFailureResponse("Invalid product id.", 400));

            var result = await _productService.GetProductByIdAsync(id, cancellationToken);
            if (result.IsFailed)
            {
                var message = result.Errors.FirstOrDefault()?.Message ?? "Product not found.";
                if (message.Contains("not found", StringComparison.OrdinalIgnoreCase))
                    return NotFound(ApiResponse.CreateFailureResponse(message, 404));

                return BadRequest(ApiResponse.CreateFailureResponse(message, 400));
            }

            return Ok(ApiResponse<ProductResponseDTO>.CreateSuccessResponse(result.Value, "Get Products Successfully"));
        }
    }
}
