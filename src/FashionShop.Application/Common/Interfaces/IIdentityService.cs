using FashionShop.Application.AuthServices.Models;
using FashionShop.Application.UserServices.DTO;
using FluentResults;

namespace FashionShop.Application.Common.Interfaces
{
    public interface IIdentityService
    {
        Task<Result<LoginIdentityResult>> LoginAsync(string username, string password);
        Task<Result<bool>> RegisterAsync(string username, string email, string firstName, string lastName, string phoneNumber, string password);

        Task<Result<UserProfileDTO>> GetProfileAsync(Guid userId);
        Task<Result<UserProfileDTO>> UpdateProfileAsync(Guid userId, UpdateUserProfileDTO dto);
        Task<Result<bool>> ChangePasswordAsync(Guid userId, ChangePasswordDTO dto);
    }

    public sealed class LoginIdentityResult
    {
        public Guid UserId { get; init; }
        public string UserName { get; init; } = string.Empty;
        public string Email { get; init; } = string.Empty;
        public string FirstName { get; init; } = string.Empty;
        public List<string> Roles { get; init; } = new();
        public List<string> Permissions { get; init; } = new();
        public string RefreshToken { get; init; } = string.Empty;
    }
}