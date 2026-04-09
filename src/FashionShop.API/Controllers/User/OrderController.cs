using FashionShop.Application.Services.OrderServices;
using FashionShop.Application.Services.OrderServices.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FashionShop.API.Controllers.User
{
    [Route("api/orders")]
    [ApiController]
    [Authorize]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public OrderController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse<OrderDTO>>> CreateOrder([FromBody] CreateOrderDTO createOrderDTO, CancellationToken cancellationToken)
        {
            var result = await _orderService.CreateOrderAsync(createOrderDTO, cancellationToken);
            if (result.IsFailed)
                return BadRequest(ApiResponse.CreateFailureResponse(result.Errors.FirstOrDefault()?.Message ?? "Create order failed", 400));

            return StatusCode(StatusCodes.Status201Created, ApiResponse<OrderDTO>.CreateSuccessResponse(result.Value, "Order successfully created."));
        }

        [HttpPost("preview")]
        public async Task<ActionResult<ApiResponse<OrderPreviewDto>>> PreviewOrder([FromBody] CreateOrderDTO createOrderDTO, CancellationToken cancellationToken)
        {
            var result = await _orderService.PreviewOderAsync(createOrderDTO, cancellationToken);
            if (result.IsFailed)
                return BadRequest(ApiResponse.CreateFailureResponse(result.Errors.FirstOrDefault()?.Message ?? "Preview failed.", 400));

            return Ok(ApiResponse<OrderPreviewDto>.CreateSuccessResponse(result.Value, "Preview Order Successfully"));
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<ApiResponse<OrderDTO>>> GetOrderById([FromRoute] Guid id, CancellationToken cancellationToken)
        {
            if (id == Guid.Empty)
                return BadRequest(ApiResponse.CreateFailureResponse("Invalid order id.", 400));

            var result = await _orderService.GetOrderByIdAsync(id, cancellationToken);
            if (result.IsFailed)
                return NotFound(ApiResponse.CreateFailureResponse(result.Errors.FirstOrDefault()?.Message ?? "Order not found.", 404));

            return Ok(ApiResponse<OrderDTO>.CreateSuccessResponse(result.Value, "Order successfully processed."));
        }

        [HttpGet("user/{userId:guid}")]
        public async Task<ActionResult<ApiResponse<List<OrderDTO>>>> GetOrdersByUserId([FromRoute] Guid userId, CancellationToken cancellationToken)
        {
            if (userId == Guid.Empty)
                return BadRequest(ApiResponse.CreateFailureResponse("Invalid user id.", 400));

            var result = await _orderService.GetOrdersByUserIdAsync(userId, cancellationToken);
            if (result.IsFailed)
                return NotFound(ApiResponse.CreateFailureResponse(result.Errors.FirstOrDefault()?.Message ?? "Orders not found.", 404));

            return Ok(ApiResponse<List<OrderDTO>>.CreateSuccessResponse(result.Value, "Order successfully processed."));
        }

        [HttpPatch("{id:guid}/cancel")]
        public async Task<ActionResult<ApiResponse>> CancelOrder([FromRoute] Guid id, CancellationToken cancellationToken)
        {
            if (id == Guid.Empty)
                return BadRequest(ApiResponse.CreateFailureResponse("Invalid order id.", 400));

            var result = await _orderService.CancelOrderAsync(id, cancellationToken);
            if (result.IsFailed)
                return BadRequest(ApiResponse.CreateFailureResponse(result.Errors.FirstOrDefault()?.Message ?? "Cancel failed.", 400));

            return Ok(ApiResponse.CreateSuccessResponse("Cancel Order Success"));
        }
    }
}
