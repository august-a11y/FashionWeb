using FashionShop.Application.Services.AuthServices.Models;
using FluentResults;

namespace FashionShop.Application.Services.AuthServices
{
    public interface IAuthenticatedService
    {
        Task<Result<AuthenticatedResponse>> LoginAsync(string username, string password);
        Task<Result<AuthenticatedResponse>> RefreshTokenAsync(string accessToken, string refreshToken);
        Task<Result<bool>> RegisterAsync(string username, string email, string firstName, string lastName, string phoneNumber, string password);
    }
}
