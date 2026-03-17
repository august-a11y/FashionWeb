using FashionShop.Application.Auth.Models;
using FluentResults;

namespace FashionShop.Application.Auth
{
    public interface IAuthenticatedService
    {
        Task<Result<AuthenticatedResponse>> LoginAsync(string username, string password);
        Task<Result<bool>> RegisterAsync(string username, string email, string firstName, string lastName, string phoneNumber, string password);
    }
}
