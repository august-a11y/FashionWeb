using FashionShop.Application.Services.AuthServices.Models;
using FashionShop.Application.Services.UserServices.DTO;
using FluentResults;

namespace FashionShop.Application.Common.Interfaces
{
    public interface IIdentityService
    {
        Task<Result<LoginIdentityResult>> LoginAsync(string username, string password);
        Task<Result<LoginIdentityResult>> RefreshTokenAsync(Guid userId, string refreshToken);
        Task<Result<bool>> RegisterAsync(string username, string email, string firstName, string lastName, string phoneNumber, string password);
        Task<string> GetRoleByUserIdAsync(Guid userId);
        Task<Result<UserProfileDTO>> GetProfileAsync(Guid userId);
        Task<Result<UserProfileDTO>> UpdateProfileAsync(Guid userId, UpdateUserProfileDTO dto);
        Task<Result<bool>> ChangePasswordAsync(Guid userId, ChangePasswordDTO dto);
    }
}