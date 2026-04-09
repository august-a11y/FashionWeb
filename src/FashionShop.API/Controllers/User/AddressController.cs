using FashionShop.Application.Services.AddressServices;
using FashionShop.Application.Services.AddressServices.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FashionShop.API.Controllers.User
{
    [ApiController]
    [Route("api/addresses")]
    [Authorize]
    public class AddressController : ControllerBase
    {
        private readonly IAddressService _addressService;

        public AddressController(IAddressService addressService)
        {
            _addressService = addressService;
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<List<AddressDTO>>>> GetMyAddresses(CancellationToken cancellationToken)
        {
            var result = await _addressService.GetMyAddressesAsync(cancellationToken);
            if (result.IsFailed)
                return BadRequest(ApiResponse.CreateFailureResponse(result.Errors.FirstOrDefault()?.Message ?? "Failed to get addresses.", 400));

            return Ok(ApiResponse<List<AddressDTO>>.CreateSuccessResponse(result.Value, "Addresses retrieved successfully."));
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<ApiResponse<AddressDTO>>> GetMyAddressById([FromRoute] Guid id, CancellationToken cancellationToken)
        {
            var result = await _addressService.GetMyAddressByIdAsync(id, cancellationToken);
            if (result.IsFailed)
            {
                var message = result.Errors.FirstOrDefault()?.Message ?? "Address not found.";
                if (message.Contains("not found", StringComparison.OrdinalIgnoreCase))
                    return NotFound(ApiResponse.CreateFailureResponse(message, 404));

                return BadRequest(ApiResponse.CreateFailureResponse(message, 400));
            }

            return Ok(ApiResponse<AddressDTO>.CreateSuccessResponse(result.Value, "Address retrieved successfully."));
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse<AddressDTO>>> CreateMyAddress([FromBody] CreateAddressDTO dto, CancellationToken cancellationToken)
        {
            var result = await _addressService.CreateMyAddressAsync(dto, cancellationToken);
            if (result.IsFailed)
                return BadRequest(ApiResponse.CreateFailureResponse(result.Errors.FirstOrDefault()?.Message ?? "Failed to create address.", 400));

            return StatusCode(StatusCodes.Status201Created, ApiResponse<AddressDTO>.CreateSuccessResponse(result.Value, "Address created successfully."));
        }

        [HttpPut("{id:guid}")]
        public async Task<ActionResult<ApiResponse<AddressDTO>>> UpdateMyAddress([FromRoute] Guid id, [FromBody] UpdateAddressDTO dto, CancellationToken cancellationToken)
        {
            var result = await _addressService.UpdateMyAddressAsync(id, dto, cancellationToken);
            if (result.IsFailed)
            {
                var message = result.Errors.FirstOrDefault()?.Message ?? "Failed to update address.";
                if (message.Contains("not found", StringComparison.OrdinalIgnoreCase))
                    return NotFound(ApiResponse.CreateFailureResponse(message, 404));

                return BadRequest(ApiResponse.CreateFailureResponse(message, 400));
            }

            return Ok(ApiResponse<AddressDTO>.CreateSuccessResponse(result.Value, "Address updated successfully."));
        }

        [HttpDelete("{id:guid}")]
        public async Task<ActionResult<ApiResponse>> DeleteMyAddress([FromRoute] Guid id, CancellationToken cancellationToken)
        {
            var result = await _addressService.DeleteMyAddressAsync(id, cancellationToken);
            if (result.IsFailed)
            {
                var message = result.Errors.FirstOrDefault()?.Message ?? "Failed to delete address.";
                if (message.Contains("not found", StringComparison.OrdinalIgnoreCase))
                    return NotFound(ApiResponse.CreateFailureResponse(message, 404));

                return BadRequest(ApiResponse.CreateFailureResponse(message, 400));
            }

            return Ok(ApiResponse.CreateSuccessResponse("Address deleted successfully."));
        }
    }
}
