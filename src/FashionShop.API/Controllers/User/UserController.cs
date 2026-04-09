using FashionShop.Application.Services.UserServices;
using FashionShop.Application.Services.UserServices.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FashionShop.API.Controllers.User
{
    [ApiController]
    [Route("api/users")]
    [Authorize]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet("me")]
        public async Task<ActionResult<ApiResponse<UserProfileDTO>>> GetMyProfile(CancellationToken cancellationToken)
        {
            var result = await _userService.GetMyProfileAsync(cancellationToken);
            if (result.IsFailed)
                return BadRequest(ApiResponse.CreateFailureResponse(result.Errors.FirstOrDefault()?.Message ?? "Get profile failed.", 400));

            return Ok(ApiResponse<UserProfileDTO>.CreateSuccessResponse(result.Value, "Profile retrieved successfully."));
        }

        [HttpPut("me")]
        public async Task<ActionResult<ApiResponse<UserProfileDTO>>> UpdateMyProfile([FromBody] UpdateUserProfileDTO dto, CancellationToken cancellationToken)
        {
            var result = await _userService.UpdateMyProfileAsync(dto, cancellationToken);
            if (result.IsFailed)
                return BadRequest(ApiResponse.CreateFailureResponse(result.Errors.FirstOrDefault()?.Message ?? "Update profile failed.", 400));

            return Ok(ApiResponse<UserProfileDTO>.CreateSuccessResponse(result.Value, "Profile updated successfully."));
        }

        [HttpPatch("me/password")]
        public async Task<ActionResult<ApiResponse>> ChangeMyPassword([FromBody] ChangePasswordDTO dto, CancellationToken cancellationToken)
        {
            var result = await _userService.ChangeMyPasswordAsync(dto, cancellationToken);
            if (result.IsFailed)
                return BadRequest(ApiResponse.CreateFailureResponse(result.Errors.FirstOrDefault()?.Message ?? "Change password failed.", 400));

            return Ok(ApiResponse.CreateSuccessResponse("Password changed successfully."));
        }
    }
}
