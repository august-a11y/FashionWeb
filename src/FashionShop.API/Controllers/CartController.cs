using FashionShop.Application.CartService;
using FashionShop.Application.CartService.DTO;
using Microsoft.AspNetCore.Mvc;

namespace FashionShop.API.Controllers
{
    [ApiController]
    [Route("api/cart")]
    public class CartController : ControllerBase
    {
        private readonly ICartService _cartService;

        public CartController(ICartService cartService)
        {
            _cartService = cartService;
        }

        [HttpGet("get")]
        public async Task<IActionResult> GetCart(CancellationToken cancellationToken)
        {
            var result = await _cartService.GetCartAsync(cancellationToken);
            if (result.IsFailed)
                return StatusCode(StatusCodes.Status404NotFound, ApiResponse.CreateFailureResponse("Not Found", 404));

            return Ok(ApiResponse<CartDTO>.CreateSuccessResponse(result.Value));
        }

        [HttpPost("add")]
        public async Task<IActionResult> AddToCart([FromBody] CartItemCreateDTO dto, CancellationToken cancellationToken)
        {
            var result = await _cartService.AddItemToCartAsync(dto, cancellationToken);
            if (result.IsFailed)
                return BadRequest(ApiResponse.CreateFailureResponse(result.Errors.FirstOrDefault()?.Message ?? "Add failed", 400));

            return Ok(ApiResponse<bool>.CreateSuccessResponse(result.Value));
        }

        [HttpDelete("remove")]
        public async Task<IActionResult> RemoveFromCart([FromBody] CartItemRemoveDTO dto, CancellationToken cancellationToken)
        {
            var result = await _cartService.RemoveItemFromCartAsync(dto.ProductId, dto.VariantId, cancellationToken);
            if (result.IsFailed)
                return BadRequest(ApiResponse.CreateFailureResponse(result.Errors.FirstOrDefault()?.Message ?? "Remove failed", 400));

            return Ok(ApiResponse<bool>.CreateSuccessResponse(result.Value));
        }

        [HttpPatch("decrease")]
        public async Task<IActionResult> DecreaseQuantity([FromBody] CartItemUpdateDTO dto, CancellationToken cancellationToken)
        {
            var result = await _cartService.DecreaseQuantityItemFromCartAsync(dto, cancellationToken);
            if (result.IsFailed)
                return BadRequest(ApiResponse.CreateFailureResponse(result.Errors.FirstOrDefault()?.Message ?? "Decrease failed", 400));

            return Ok(ApiResponse<bool>.CreateSuccessResponse(result.Value));
        }
    }
}
