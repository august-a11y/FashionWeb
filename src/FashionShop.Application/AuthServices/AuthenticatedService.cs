using FashionShop.Application.AuthServices.Models;
using FashionShop.Application.Common.Interfaces;
using FashionShop.Domain.Constants;
using FluentResults;
using Microsoft.IdentityModel.JsonWebTokens;
using System.Security.Claims;
using System.Text.Json;

namespace FashionShop.Application.AuthServices
{
    public class AuthenticatedService : IAuthenticatedService
    {
        private readonly IJwtService _jwtService;
        private readonly IIdentityService _identityService;

        public AuthenticatedService(IJwtService jwtService, IIdentityService identityService)
        {
            _jwtService = jwtService;
            _identityService = identityService;
        }

        public async Task<Result<AuthenticatedResponse>> LoginAsync(string username, string password)
        {
            var loginResult = await _identityService.LoginAsync(username, password);
            if (loginResult.IsFailed)
            {
                return Result.Fail(loginResult.Errors.First().Message);
            }

            var user = loginResult.Value;

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(UserClaims.FirstName, user.FirstName),
                new Claim(UserClaims.Id, user.UserId.ToString()),
                new Claim(UserClaims.Roles, string.Join(";", user.Roles)),
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(UserClaims.Permissions, JsonSerializer.Serialize(user.Permissions)),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var token = _jwtService.GenerateToken(claims);

            return Result.Ok(new AuthenticatedResponse
            {
                AccessToken = token,
                RefreshToken = user.RefreshToken
            });
        }

        public Task<Result<bool>> RegisterAsync(
            string username,
            string email,
            string firstName,
            string lastName,
            string phoneNumber,
            string password)
        {
            return _identityService.RegisterAsync(username, email, firstName, lastName, phoneNumber, password);
        }
    }
}
