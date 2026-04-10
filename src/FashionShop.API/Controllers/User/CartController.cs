using FashionShop.Application.Services.CartServices;
using FashionShop.Application.Services.CartServices.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FashionShop.API.Controllers.User
{
    [ApiController]
    [Route("api/carts")]

    public class CartController : ControllerBase
    {
        private readonly ICartService _cartService;

        public CartController(ICartService cartService)
        {
            _cartService = cartService;
        }

        [HttpGet("me")]
        public async Task<ActionResult<ApiResponse<CartDTO>>> GetCart(CancellationToken cancellationToken)
        {
            var result = await _cartService.GetCartAsync(cancellationToken);
            if (result.IsFailed)
                return StatusCode(StatusCodes.Status404NotFound, ApiResponse.CreateFailureResponse("Not Found", 404));

            return Ok(ApiResponse<CartDTO>.CreateSuccessResponse(result.Value, "Get Products Successfully"));
        }

        [HttpPost("me/items")]
        public async Task<ActionResult<ApiResponse>> AddToCart([FromBody] CartItemCreateDTO dto, CancellationToken cancellationToken)
        {
            var result = await _cartService.AddItemToCartAsync(dto, cancellationToken);
            if (result.IsFailed)
                return BadRequest(ApiResponse.CreateFailureResponse(result.Errors.FirstOrDefault()?.Message ?? "Add failed", 400));

            return Ok(ApiResponse.CreateSuccessResponse("Item added to cart successfully."));
        }

        [HttpDelete("me/items")]
        public async Task<ActionResult<ApiResponse>> RemoveFromCart([FromBody] CartItemRemoveDTO dto, CancellationToken cancellationToken)
        {
            var result = await _cartService.RemoveItemFromCartAsync(dto.ProductId, dto.VariantId, cancellationToken);
            if (result.IsFailed)
                return BadRequest(ApiResponse.CreateFailureResponse(result.Errors.FirstOrDefault()?.Message ?? "Remove failed", 400));

            return Ok(ApiResponse.CreateSuccessResponse("Item removed from cart successfully."));
        }

        [HttpPatch("me/items")]
        public async Task<ActionResult<ApiResponse>> UpdateQuantity([FromBody] CartItemUpdateDTO dto, CancellationToken cancellationToken)
        {
            var result = await _cartService.UpdateQuantityItemFromCartAsync(dto, cancellationToken);
            if (result.IsFailed)
                return BadRequest(ApiResponse.CreateFailureResponse(result.Errors.FirstOrDefault()?.Message ?? "Update failed", 400));
            return Ok(ApiResponse.CreateSuccessResponse("Quantity updated successfully"));
        }
    }
}
