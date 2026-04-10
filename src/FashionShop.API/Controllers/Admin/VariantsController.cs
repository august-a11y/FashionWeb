using FashionShop.Application.Services.VariantServices;
using FashionShop.Application.Services.VariantServices.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FashionShop.API.Controllers.Admin
{
    [ApiController]
    [Route("api/admin/variants")]
    [Authorize(Policy = "Admin")]
    public class VariantsController : ControllerBase
    {
        private readonly IVariantService _variantService;

        public VariantsController(IVariantService variantService)
        {
            _variantService = variantService;
        }

        [HttpPost]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult<ApiResponse<VariantDTO>>> Create([FromForm] CreateVariantDTO createVariantDto, CancellationToken cancellationToken)
        {
            var result = await _variantService.CreateVariantAsync(createVariantDto, cancellationToken);
            if (result.IsFailed)
                return BadRequest(ApiResponse.CreateFailureResponse(result.Errors.FirstOrDefault()?.Message ?? "Failed to create variant.", 400));

            return StatusCode(StatusCodes.Status201Created, ApiResponse<VariantDTO>.CreateSuccessResponse(result.Value, "Variant created successfully."));
        }

        [HttpPut("{id:guid}")]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult<ApiResponse<VariantDTO>>> Update([FromRoute] Guid id, [FromForm] UpdateVariantDTO updateVariantDto, CancellationToken cancellationToken)
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
        public async Task<ActionResult<ApiResponse>> Delete([FromRoute] Guid id, CancellationToken cancellationToken)
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
