using FashionShop.Application.Services.AuthServices;
using FashionShop.Application.Services.AuthServices.Models;
using Microsoft.AspNetCore.Mvc;

namespace FashionShop.API.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthenticationController : ControllerBase
    {
        private readonly IAuthenticatedService _authenticatedService;

        public AuthenticationController(IAuthenticatedService authenticatedService)
        {
            _authenticatedService = authenticatedService;
        }

        [HttpPost("login")]
        public async Task<ActionResult<ApiResponse<AuthenticatedResponse>>> Login([FromBody] LoginRequestDTO login)
        {
            var result = await _authenticatedService.LoginAsync(login.Username, login.Password);
            if (result.IsFailed)
            {
                var message = result.Errors.FirstOrDefault()?.Message ?? "Login failed.";
                return BadRequest(ApiResponse.CreateFailureResponse(message, 400));
            }

            return Ok(ApiResponse<AuthenticatedResponse>.CreateSuccessResponse(result.Value, "Login successful."));
        }

        [HttpPost("register")]
        public async Task<ActionResult<ApiResponse>> Register([FromBody] RegisterRequestDTO register)
        {
            var result = await _authenticatedService.RegisterAsync(
                register.Username,
                register.Email,
                register.FirstName,
                register.LastName,
                register.PhoneNumber,
                register.Password);

            if (result.IsFailed)
            {
                var message = result.Errors.FirstOrDefault()?.Message ?? "Registration failed.";
                return BadRequest(ApiResponse.CreateFailureResponse(message, 400));
            }

            return Ok(ApiResponse.CreateSuccessResponse("Registration successful."));
        }

        [HttpPost("refresh-token")]
        public async Task<ActionResult<ApiResponse<AuthenticatedResponse>>> RefreshToken([FromBody] RefreshTokenRequestDTO request)
        {
            var result = await _authenticatedService.RefreshTokenAsync(request.AccessToken, request.RefreshToken);
            if (result.IsFailed)
            {
                var message = result.Errors.FirstOrDefault()?.Message ?? "Refresh token failed.";
                return Unauthorized(ApiResponse.CreateFailureResponse(message, 401));
            }

            return Ok(ApiResponse<AuthenticatedResponse>.CreateSuccessResponse(result.Value, "Refresh token successful."));
        }
    }
}
