using FashionShop.Application.VariantServices;
using FashionShop.Application.VariantServices.DTO;
using Microsoft.AspNetCore.Mvc;

namespace FashionShop.API.Controllers
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
        public async Task<IActionResult> GetByProductId([FromRoute] Guid productId, CancellationToken cancellationToken)
        {
            var result = await _variantService.GetVariantsByProductIdAsync(productId, cancellationToken);
            if (result.IsFailed)
                return BadRequest(ApiResponse.CreateFailureResponse(result.Errors.FirstOrDefault()?.Message ?? "Failed to get variants.", 400));

            return Ok(ApiResponse<List<VariantDTO>>.CreateSuccessResponse(result.Value));
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById([FromRoute] Guid id, CancellationToken cancellationToken)
        {
            var result = await _variantService.GetVariantByIdAsync(id, cancellationToken);
            if (result.IsFailed)
            {
                var message = result.Errors.FirstOrDefault()?.Message ?? "Variant not found.";
                if (message.Contains("not found", StringComparison.OrdinalIgnoreCase))
                    return NotFound(ApiResponse.CreateFailureResponse(message, 404));

                return BadRequest(ApiResponse.CreateFailureResponse(message, 400));
            }

            return Ok(ApiResponse<VariantDTO>.CreateSuccessResponse(result.Value));
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateVariantDTO createVariantDto, CancellationToken cancellationToken)
        {
            var result = await _variantService.CreateVariantAsync(createVariantDto, cancellationToken);
            if (result.IsFailed)
                return BadRequest(ApiResponse.CreateFailureResponse(result.Errors.FirstOrDefault()?.Message ?? "Failed to create variant.", 400));

            return StatusCode(StatusCodes.Status201Created, ApiResponse<VariantDTO>.CreateSuccessResponse(result.Value, "Variant created successfully."));
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] UpdateVariantDTO updateVariantDto, CancellationToken cancellationToken)
        {
            var result = await _variantService.UpdateVariantAsync(id, updateVariantDto, cancellationToken);
            if (result.IsFailed)
            {
                var message = result.Errors.FirstOrDefault()?.Message ?? "Failed to update variant.";
                if (message.Contains("not found", StringComparison.OrdinalIgnoreCase))
                    return NotFound(ApiResponse.CreateFailureResponse(message, 404));

                return BadRequest(ApiResponse.CreateFailureResponse(message, 400));
            }

            return Ok(ApiResponse<VariantDTO>.CreateSuccessResponse(result.Value, "Variant updated successfully."));
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete([FromRoute] Guid id, CancellationToken cancellationToken)
        {
            var result = await _variantService.DeleteVariantAsync(id, cancellationToken);
            if (result.IsFailed)
            {
                var message = result.Errors.FirstOrDefault()?.Message ?? "Failed to delete variant.";
                if (message.Contains("not found", StringComparison.OrdinalIgnoreCase))
                    return NotFound(ApiResponse.CreateFailureResponse(message, 404));

                return BadRequest(ApiResponse.CreateFailureResponse(message, 400));
            }

            return Ok(ApiResponse.CreateSuccessResponse("Variant deleted successfully."));
        }
    }
}