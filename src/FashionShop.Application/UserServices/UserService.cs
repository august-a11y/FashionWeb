using FashionShop.Application.Common.Interfaces;
using FashionShop.Application.UserServices.DTO;
using FashionShop.Domain.Identity;
using FluentResults;
using Microsoft.AspNetCore.Identity;

namespace FashionShop.Application.UserServices
{
    public class UserService : IUserService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly IRequestContext _requestContext;

        public UserService(UserManager<AppUser> userManager, IRequestContext requestContext, SignInManager<AppUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _requestContext = requestContext;
        }

        public async Task<Result<UserProfileDTO>> GetMyProfileAsync(CancellationToken cancellationToken)
        {
            var userResult = await GetCurrentUserAsync();
            if (userResult.IsFailed)
                return Result.Fail<UserProfileDTO>(userResult.Errors.First().Message);

            return Result.Ok(MapToDto(userResult.Value));
        }

        public async Task<Result<UserProfileDTO>> UpdateMyProfileAsync(UpdateUserProfileDTO updateUserProfileDTO, CancellationToken cancellationToken)
        {
            var userResult = await GetCurrentUserAsync();
            if (userResult.IsFailed)
                return Result.Fail<UserProfileDTO>(userResult.Errors.First().Message);

            var user = userResult.Value;

            if (!string.IsNullOrWhiteSpace(updateUserProfileDTO.Email) &&
                !string.Equals(user.Email, updateUserProfileDTO.Email, StringComparison.OrdinalIgnoreCase))
            {
                var existedUser = await _userManager.FindByEmailAsync(updateUserProfileDTO.Email);
                if (existedUser != null && existedUser.Id != user.Id)
                    return Result.Fail<UserProfileDTO>("Email already exists.");

                user.Email = updateUserProfileDTO.Email;
                user.NormalizedEmail = updateUserProfileDTO.Email.ToUpperInvariant();
            }

            if (!string.IsNullOrWhiteSpace(updateUserProfileDTO.FirstName))
                user.FirstName = updateUserProfileDTO.FirstName;

            if (!string.IsNullOrWhiteSpace(updateUserProfileDTO.LastName))
                user.LastName = updateUserProfileDTO.LastName;

            if (!string.IsNullOrWhiteSpace(updateUserProfileDTO.PhoneNumber))
                user.PhoneNumber = updateUserProfileDTO.PhoneNumber;

            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                var errors = string.Join(", ", updateResult.Errors.Select(e => e.Description));
                return Result.Fail<UserProfileDTO>($"Update profile failed: {errors}");
            }

            return Result.Ok(MapToDto(user));
        }

        public async Task<Result<bool>> ChangeMyPasswordAsync(ChangePasswordDTO changePasswordDTO, CancellationToken cancellationToken)
        {
            var userResult = await GetCurrentUserAsync();
            if (userResult.IsFailed)
                return Result.Fail<bool>(userResult.Errors.First().Message);

            if (string.IsNullOrWhiteSpace(changePasswordDTO.CurrentPassword) ||
                string.IsNullOrWhiteSpace(changePasswordDTO.NewPassword))
            {
                return Result.Fail<bool>("CurrentPassword and NewPassword are required.");
            }

            var user = userResult.Value;
            var changeResult = await _userManager.ChangePasswordAsync(
                user,
                changePasswordDTO.CurrentPassword,
                changePasswordDTO.NewPassword);
            if (changeResult.Succeeded)
            {
                await _userManager.ResetAccessFailedCountAsync(user);
                await _signInManager.RefreshSignInAsync(user);
                return Result.Ok(true);
            }
            if(changeResult.Errors.Any(e => e.Code == "PasswordMismatch"))
            {
                await _userManager.AccessFailedAsync(user);

                if(await _userManager.IsLockedOutAsync(user))
                {
                    var lockoutEnd = await _userManager.GetLockoutEndDateAsync(user);
                    await _signInManager.SignOutAsync();
                    return Result.Fail<bool>("Your account has been locked due to multiple failed password attempts. Please try again later.");
                }

                var failedCount = await _userManager.GetAccessFailedCountAsync(user);
                var maxFailedAccessAttempts = _userManager.Options.Lockout.MaxFailedAccessAttempts;

                return Result.Fail<bool>($"Invalid password. You have {maxFailedAccessAttempts - failedCount} attempts left before your account gets locked.");
            }

            
            var errors = string.Join(", ", changeResult.Errors.Select(e => e.Description));
            return Result.Fail<bool>($"Change password failed: {errors}");
            
            

            
        }

        private async Task<Result<AppUser>> GetCurrentUserAsync()
        {
            if (!_requestContext.UserId.HasValue)
                return Result.Fail<AppUser>("User is not authenticated.");

            var user = await _userManager.FindByIdAsync(_requestContext.UserId.Value.ToString());
            if (user == null)
                return Result.Fail<AppUser>("User not found.");

            return Result.Ok(user);
        }

        private static UserProfileDTO MapToDto(AppUser user)
        {
            return new UserProfileDTO
            {
                UserName = user.UserName ?? string.Empty,
                Email = user.Email ?? string.Empty,
                FirstName = user.FirstName,
                LastName = user.LastName,
                PhoneNumber = user.PhoneNumber ?? string.Empty,
                IsActive = user.IsActive
            };
        }
    }
}