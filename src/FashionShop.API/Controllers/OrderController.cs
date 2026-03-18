using FashionShop.Application.OrderServices;
using FashionShop.Application.OrderServices.DTO;
using FashionShop.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FashionShop.API.Controllers
{
    [Route("api/order")]
    [ApiController]
    [Authorize]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public OrderController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderDTO createOrderDTO, CancellationToken cancellationToken)
        {
            var result = await _orderService.CreateOrderAsync(createOrderDTO, cancellationToken);
            if (result.IsFailed)
                return BadRequest(ApiResponse.CreateFailureResponse(result.Errors.FirstOrDefault()?.Message ?? "Create order failed", 400));

            return Ok(ApiResponse<OrderDTO>.CreateSuccessResponse(result.Value));
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetOrderById([FromRoute] Guid id, CancellationToken cancellationToken)
        {
            if (id == Guid.Empty)
                return BadRequest(ApiResponse.CreateFailureResponse("Invalid order id.", 400));

            var result = await _orderService.GetOrderByIdAsync(id, cancellationToken);
            if (result.IsFailed)
                return NotFound(ApiResponse.CreateFailureResponse(result.Errors.FirstOrDefault()?.Message ?? "Order not found.", 404));

            return Ok(ApiResponse<OrderDTO>.CreateSuccessResponse(result.Value));
        }

        [HttpGet("user/{userId:guid}")]
        public async Task<IActionResult> GetOrdersByUserId([FromRoute] Guid userId, CancellationToken cancellationToken)
        {
            if (userId == Guid.Empty)
                return BadRequest(ApiResponse.CreateFailureResponse("Invalid user id.", 400));

            var result = await _orderService.GetOrdersByUserIdAsync(userId, cancellationToken);
            if (result.IsFailed)
                return NotFound(ApiResponse.CreateFailureResponse(result.Errors.FirstOrDefault()?.Message ?? "Orders not found.", 404));

            return Ok(ApiResponse<List<OrderDTO>>.CreateSuccessResponse(result.Value));
        }

        [HttpPost("preview")]
        public async Task<IActionResult> PreviewOrder([FromBody] CreateOrderDTO createOrderDTO, CancellationToken cancellationToken)
        {
            var result = await _orderService.PreviewOderAsync(createOrderDTO, cancellationToken);
            if (result.IsFailed)
                return BadRequest(ApiResponse.CreateFailureResponse(result.Errors.FirstOrDefault()?.Message ?? "Preview failed.", 400));

            return Ok(ApiResponse<OrderPreviewDto>.CreateSuccessResponse(result.Value));
        }

        [HttpPatch("{id:guid}/cancel")]
        public async Task<IActionResult> CancelOrder([FromRoute] Guid id, CancellationToken cancellationToken)
        {
            if (id == Guid.Empty)
                return BadRequest(ApiResponse.CreateFailureResponse("Invalid order id.", 400));

            var result = await _orderService.CancelOrderAsync(id, cancellationToken);
            if (result.IsFailed)
                return BadRequest(ApiResponse.CreateFailureResponse(result.Errors.FirstOrDefault()?.Message ?? "Cancel failed.", 400));

            return Ok(ApiResponse<bool>.CreateSuccessResponse(result.Value));
        }


        [HttpPatch("{id:guid}/status")]
        public async Task<IActionResult> UpdateOrderStatus(
            [FromRoute] Guid id,
            [FromBody] OrderStatusUpdateDTO request,
            CancellationToken cancellationToken)
        {
            if (id == Guid.Empty)
                return BadRequest(ApiResponse.CreateFailureResponse("Invalid order id.", 400));

            if (string.IsNullOrWhiteSpace(request.Status))
                return BadRequest(ApiResponse.CreateFailureResponse("Status is required.", 400));

            var result = await _orderService.UpdateOderStatusAsync(id, request.Status.Trim(), cancellationToken);
            if (result.IsFailed)
            {
                var message = result.Errors.FirstOrDefault()?.Message ?? "Update status failed.";
                return HandleUpdateStatusFailure(message);
            }

            return Ok(ApiResponse<bool>.CreateSuccessResponse(result.Value));
        }

        private IActionResult HandleUpdateStatusFailure(string message)
        {
            if (string.Equals(message, "Order does not exist.", StringComparison.OrdinalIgnoreCase))
            {
                return NotFound(ApiResponse.CreateFailureResponse("Order not found.", 404));
            }

            if (string.Equals(message, "Status is required.", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(message, "Invalid order status.", StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest(ApiResponse.CreateFailureResponse("Invalid order status.", 400));
            }

            if (message.StartsWith("Invalid status transition:", StringComparison.OrdinalIgnoreCase))
            {
                var friendlyMessage = BuildTransitionMessage(message);
                return Conflict(ApiResponse.CreateFailureResponse(friendlyMessage, 409));
            }

            return BadRequest(ApiResponse.CreateFailureResponse(message, 400));
        }

        private static string BuildTransitionMessage(string rawMessage)
        {
            // Example raw message: "Invalid status transition: Cancelled -> Delivered."
            var transitionPart = rawMessage.Replace("Invalid status transition:", string.Empty, StringComparison.OrdinalIgnoreCase)
                                           .Trim()
                                           .TrimEnd('.');

            var parts = transitionPart.Split("->", StringSplitOptions.TrimEntries);
            if (parts.Length != 2)
            {
                return "Invalid order status transition.";
            }

            var from = parts[0];
            var to = parts[1];

            return (from, to) switch
            {
                ("Cancelled", "Delivered") => "A cancelled order cannot be marked as delivered.",
                ("Cancelled", "Shipped") => "A cancelled order cannot be marked as shipped.",
                ("Cancelled", "Processing") => "A cancelled order cannot be moved back to processing.",
                ("Delivered", "Cancelled") => "A delivered order cannot be cancelled.",
                ("Delivered", "Processing") => "A delivered order cannot be moved back to processing.",
                ("Delivered", "Shipped") => "A delivered order cannot be moved back to shipped.",
                ("Pending", "Delivered") => "A pending order cannot be moved directly to delivered.",
                ("Pending", "Shipped") => "A pending order cannot be moved directly to shipped.",
                ("Processing", "Delivered") => "A processing order must be shipped before delivery.",
                _ => $"Cannot transition order status from '{from}' to '{to}'."
            };
        }
    }
}
