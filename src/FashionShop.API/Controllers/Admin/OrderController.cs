using FashionShop.Application.Services.OrderServices;
using FashionShop.Application.Services.OrderServices.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FashionShop.API.Controllers.Admin
{
    [Route("api/admin/orders")]
    [ApiController]
    [Authorize(Policy = "Admin")]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public OrderController(IOrderService orderService)
        {
            _orderService = orderService;
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
        [HttpGet]
        public async Task<ActionResult<ApiResponse<List<OrderDTO>>>> GetAllOrders(CancellationToken cancellationToken)
        {
            var result = await _orderService.GetAllOrdersAsync(cancellationToken);
            if (result.IsFailed)
                return NotFound(ApiResponse.CreateFailureResponse(result.Errors.FirstOrDefault()?.Message ?? "Orders not found.", 404));
            return Ok(ApiResponse<List<OrderDTO>>.CreateSuccessResponse(result.Value, "Orders successfully processed."));
        }

        [HttpPatch("{id:guid}/status")]
        public async Task<ActionResult<ApiResponse>> UpdateOrderStatus(
            [FromRoute] Guid id,
            [FromBody] OrderStatusUpdateDTO request,
            CancellationToken cancellationToken)
        {
            if (id == Guid.Empty)
                return BadRequest(ApiResponse.CreateFailureResponse("Invalid order id.", 400));

            if (!request.Status.HasValue)
                return BadRequest(ApiResponse.CreateFailureResponse("Status is required.", 400));

            var result = await _orderService.UpdateOderStatusAsync(id, request.Status.Value, cancellationToken);
            if (result.IsFailed)
            {
                var message = result.Errors.FirstOrDefault()?.Message ?? "Update status failed.";
                return HandleUpdateStatusFailure(message);
            }

            return Ok(ApiResponse.CreateSuccessResponse("Order status updated successfully. "));
        }

        private ActionResult<ApiResponse> HandleUpdateStatusFailure(string message)
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
                ("Cancelled", "Shipping") => "A cancelled order cannot be marked as Shipping.",
                ("Cancelled", "Processing") => "A cancelled order cannot be moved back to processing.",
                ("Delivered", "Cancelled") => "A delivered order cannot be cancelled.",
                ("Delivered", "Processing") => "A delivered order cannot be moved back to processing.",
                ("Delivered", "Shipping") => "A delivered order cannot be moved back to Shipping.",
                ("Pending", "Delivered") => "A pending order cannot be moved directly to delivered.",
                ("Pending", "Shipping") => "A pending order cannot be moved directly to Shipping.",
                ("Processing", "Delivered") => "A processing order must be Shipping before delivery.",
                _ => $"Cannot transition order status from '{from}' to '{to}'."
            };
        }
    }
}
