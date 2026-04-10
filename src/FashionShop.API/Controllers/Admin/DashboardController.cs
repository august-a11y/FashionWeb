using FashionShop.Application.Services.DashboardServices;
using FashionShop.Application.Services.DashboardServices.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FashionShop.API.Controllers.Admin
{
    [ApiController]
    [Route("api/admin/dashboard")]
    [Authorize(Policy = "Admin")]
    public class DashboardController : ControllerBase
    {
        private readonly IDashboardService _dashboardService;

        public DashboardController(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        [HttpGet("overview")]
        public async Task<ActionResult<ApiResponse<DashboardOverviewDTO>>> GetOverview(
            [FromQuery] DashboardPeriod period = DashboardPeriod.Month,
            [FromQuery] DateTime? from = null,
            [FromQuery] DateTime? to = null,
            [FromQuery] int topN = 5,
            [FromQuery] int recentLimit = 5,
            CancellationToken cancellationToken = default)
        {
            if (from.HasValue ^ to.HasValue)
            {
                return BadRequest(ApiResponse.CreateFailureResponse("Both 'from' and 'to' must be provided together.", 400));
            }

            if (from.HasValue && to.HasValue && to.Value <= from.Value)
            {
                return BadRequest(ApiResponse.CreateFailureResponse("'to' must be greater than 'from'.", 400));
            }

            if (topN <= 0 || recentLimit <= 0)
            {
                return BadRequest(ApiResponse.CreateFailureResponse("'topN' and 'recentLimit' must be greater than 0.", 400));
            }

            var result = await _dashboardService.GetOverviewAsync(period, from, to, topN, recentLimit, cancellationToken);
            if (result.IsFailed)
            {
                var message = result.Errors.FirstOrDefault()?.Message ?? "Failed to get dashboard overview.";
                return StatusCode(500, ApiResponse.CreateFailureResponse(message, 500));
            }

            return Ok(ApiResponse<DashboardOverviewDTO>.CreateSuccessResponse(result.Value, "OK"));
        }
    }
}
