using FashionShop.Application.Services.VariantServices;
using FashionShop.Application.Services.VariantServices.DTO;
using Microsoft.AspNetCore.Mvc;

namespace FashionShop.API.Controllers.User
{
    [ApiController]
    [Route("api/variants")]
    public class VariantsController : ControllerBase
    {
        private readonly IVariantService _variantService;

        public VariantsController(IVariantService variantService)
        {
            _variantService = variantService;
        }

        [HttpGet("~/api/products/{productId:guid}/variants")]
        public async Task<ActionResult<ApiResponse<List<VariantDTO>>>> GetByProductId([FromRoute] Guid productId, CancellationToken cancellationToken)
        {
            var result = await _variantService.GetVariantsByProductIdAsync(productId, cancellationToken);
            if (result.IsFailed)
                return BadRequest(ApiResponse.CreateFailureResponse(result.Errors.FirstOrDefault()?.Message ?? "Failed to get variants.", 400));

            return Ok(ApiResponse<List<VariantDTO>>.CreateSuccessResponse(result.Value, "Variants retrieved successfully."));
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<ApiResponse<VariantDTO>>> GetById([FromRoute] Guid id, CancellationToken cancellationToken)
        {
            var result = await _variantService.GetVariantByIdAsync(id, cancellationToken);
            if (result.IsFailed)
            {
                var message = result.Errors.FirstOrDefault()?.Message ?? "Variant not found.";
                if (message.Contains("not found", StringComparison.OrdinalIgnoreCase))
                    return NotFound(ApiResponse.CreateFailureResponse(message, 404));

                return BadRequest(ApiResponse.CreateFailureResponse(message, 400));
            }

            return Ok(ApiResponse<VariantDTO>.CreateSuccessResponse(result.Value, "Variant retrieved successfully."));
        }
    }
}
