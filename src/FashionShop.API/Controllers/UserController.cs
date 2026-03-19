using FashionShop.Application.UserServices;
using FashionShop.Application.UserServices.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FashionShop.API.Controllers
{
    [ApiController]
    [Route("api/user")]

    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet("me")]
        public async Task<IActionResult> GetMyProfile(CancellationToken cancellationToken)
        {
            var result = await _userService.GetMyProfileAsync(cancellationToken);
            if (result.IsFailed)
                return BadRequest(ApiResponse.CreateFailureResponse(result.Errors.FirstOrDefault()?.Message ?? "Get profile failed.", 400));

            return Ok(ApiResponse<UserProfileDTO>.CreateSuccessResponse(result.Value));
        }

        [HttpPut("me")]
        public async Task<IActionResult> UpdateMyProfile([FromBody] UpdateUserProfileDTO dto, CancellationToken cancellationToken)
        {
            var result = await _userService.UpdateMyProfileAsync(dto, cancellationToken);
            if (result.IsFailed)
                return BadRequest(ApiResponse.CreateFailureResponse(result.Errors.FirstOrDefault()?.Message ?? "Update profile failed.", 400));

            return Ok(ApiResponse<UserProfileDTO>.CreateSuccessResponse(result.Value, "Profile updated successfully."));
        }

        [HttpPatch("me/change-password")]
        public async Task<IActionResult> ChangeMyPassword([FromBody] ChangePasswordDTO dto, CancellationToken cancellationToken)
        {
            var result = await _userService.ChangeMyPasswordAsync(dto, cancellationToken);
            if (result.IsFailed)
                return BadRequest(ApiResponse.CreateFailureResponse(result.Errors.FirstOrDefault()?.Message ?? "Change password failed.", 400));

            return Ok(ApiResponse.CreateSuccessResponse("Password changed successfully."));
        }
    }
}