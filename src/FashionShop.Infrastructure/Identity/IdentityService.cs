using FashionShop.Application.Common.Interfaces;
using FashionShop.Application.Extensions;
using FashionShop.Application.Services.AuthServices.Models;
using FashionShop.Application.Services.UserServices.DTO;
using FashionShop.Domain.Constants;
using FashionShop.Domain.Identity;
using FluentResults;
using Microsoft.AspNetCore.Identity;
using System.Reflection;

namespace FashionShop.Infrastructure.Identity
{
    public class IdentityService : IIdentityService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly RoleManager<AppRole> _roleManager;

        public IdentityService(
            UserManager<AppUser> userManager,
            SignInManager<AppUser> signInManager,
            RoleManager<AppRole> roleManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
        }

        public async Task<Result<LoginIdentityResult>> LoginAsync(string username, string password)
        {
            var user = await _userManager.FindByNameAsync(username);
            if (user == null) return Result.Fail("Invalid username or password.");
            if (!user.IsActive) return Result.Fail("User account is inactive.");

            var signIn = await _signInManager.PasswordSignInAsync(user, password, false, true);

            if (signIn.IsLockedOut)
            {
                var lockoutEnd = await _userManager.GetLockoutEndDateAsync(user);
                var remaining = lockoutEnd.HasValue ? lockoutEnd.Value - DateTimeOffset.UtcNow : TimeSpan.Zero;
                return Result.Fail($"User account is locked. Try again in {Math.Max(0, remaining.Minutes)} minutes and {Math.Max(0, remaining.Seconds)} seconds.");
            }

            if (!signIn.Succeeded) return Result.Fail("Invalid username or password.");

            var roles = await _userManager.GetRolesAsync(user);
            var permissions = await GetPermissionsByUserIdAsync(user.Id.ToString());

            var refreshToken = Guid.NewGuid().ToString("N");
            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
            await _userManager.UpdateAsync(user);

            return Result.Ok(new LoginIdentityResult
            {
                UserId = user.Id,
                UserName = user.UserName ?? string.Empty,
                Email = user.Email ?? string.Empty,
                FirstName = user.FirstName ?? string.Empty,
                Roles = roles.ToList(),
                Permissions = permissions,
                RefreshToken = refreshToken
            });
        }

        public async Task<Result<LoginIdentityResult>> RefreshTokenAsync(Guid userId, string refreshToken)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null) return Result.Fail("User not found.");
            if (!user.IsActive) return Result.Fail("User account is inactive.");

            if (string.IsNullOrWhiteSpace(user.RefreshToken) ||
                !string.Equals(user.RefreshToken, refreshToken, StringComparison.Ordinal) ||
                user.RefreshTokenExpiryTime is null ||
                user.RefreshTokenExpiryTime <= DateTime.UtcNow)
            {
                return Result.Fail("Invalid or expired refresh token.");
            }

            var roles = await _userManager.GetRolesAsync(user);
            var permissions = await GetPermissionsByUserIdAsync(user.Id.ToString());

            var newRefreshToken = Guid.NewGuid().ToString("N");
            user.RefreshToken = newRefreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
            await _userManager.UpdateAsync(user);

            return Result.Ok(new LoginIdentityResult
            {
                UserId = user.Id,
                UserName = user.UserName ?? string.Empty,
                Email = user.Email ?? string.Empty,
                FirstName = user.FirstName ?? string.Empty,
                Roles = roles.ToList(),
                Permissions = permissions,
                RefreshToken = newRefreshToken
            });
        }

        public async Task<Result<bool>> RegisterAsync(string username, string email, string firstName, string lastName, string phoneNumber, string password)
        {
            if (await _userManager.FindByNameAsync(username) != null) return Result.Fail("User already exists!");
            if (await _userManager.FindByEmailAsync(email) != null) return Result.Fail("Email already exists!");

            var user = new AppUser
            {
                Email = email,
                NormalizedEmail = email.ToUpperInvariant(),
                SecurityStamp = Guid.NewGuid().ToString(),
                UserName = username,
                NormalizedUserName = username.ToUpperInvariant(),
                FirstName = firstName,
                LastName = lastName,
                PhoneNumber = phoneNumber
            };

            var created = await _userManager.CreateAsync(user, password);
            if (!created.Succeeded) return Result.Fail($"Register Fail: {string.Join(", ", created.Errors.Select(e => e.Description))}");

            await _userManager.AddToRoleAsync(user, "User");
            return Result.Ok(true);
        }

        public async Task<Result<UserProfileDTO>> GetProfileAsync(Guid userId)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null) return Result.Fail("User not found.");

            return Result.Ok(new UserProfileDTO
            {
                UserName = user.UserName ?? string.Empty,
                Email = user.Email ?? string.Empty,
                FirstName = user.FirstName,
                LastName = user.LastName,
                PhoneNumber = user.PhoneNumber ?? string.Empty,
                IsActive = user.IsActive
            });
        }

        public async Task<Result<UserProfileDTO>> UpdateProfileAsync(Guid userId, UpdateUserProfileDTO dto)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null) return Result.Fail("User not found.");

            if (!string.IsNullOrWhiteSpace(dto.Email) && !string.Equals(user.Email, dto.Email, StringComparison.OrdinalIgnoreCase))
            {
                var existed = await _userManager.FindByEmailAsync(dto.Email);
                if (existed != null && existed.Id != user.Id) return Result.Fail("Email already exists.");
                user.Email = dto.Email;
                user.NormalizedEmail = dto.Email.ToUpperInvariant();
            }

            if (!string.IsNullOrWhiteSpace(dto.FirstName)) user.FirstName = dto.FirstName;
            if (!string.IsNullOrWhiteSpace(dto.LastName)) user.LastName = dto.LastName;
            if (!string.IsNullOrWhiteSpace(dto.PhoneNumber)) user.PhoneNumber = dto.PhoneNumber;

            var updated = await _userManager.UpdateAsync(user);
            if (!updated.Succeeded) return Result.Fail($"Update profile failed: {string.Join(", ", updated.Errors.Select(e => e.Description))}");

            return await GetProfileAsync(userId);
        }

        public async Task<Result<bool>> ChangePasswordAsync(Guid userId, ChangePasswordDTO dto)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null) return Result.Fail("User not found.");
            if (string.IsNullOrWhiteSpace(dto.CurrentPassword) || string.IsNullOrWhiteSpace(dto.NewPassword))
                return Result.Fail("CurrentPassword and NewPassword are required.");

            var changed = await _userManager.ChangePasswordAsync(user, dto.CurrentPassword, dto.NewPassword);
            if (changed.Succeeded)
            {
                await _userManager.ResetAccessFailedCountAsync(user);
                await _signInManager.RefreshSignInAsync(user);
                return Result.Ok(true);
            }

            if (changed.Errors.Any(e => e.Code == "PasswordMismatch"))
            {
                await _userManager.AccessFailedAsync(user);
                if (await _userManager.IsLockedOutAsync(user))
                {
                    await _signInManager.SignOutAsync();
                    return Result.Fail("Your account has been locked due to multiple failed password attempts. Please try again later.");
                }

                var failedCount = await _userManager.GetAccessFailedCountAsync(user);
                var maxAttempts = _userManager.Options.Lockout.MaxFailedAccessAttempts;
                return Result.Fail($"Invalid password. You have {maxAttempts - failedCount} attempts left before your account gets locked.");
            }

            return Result.Fail($"Change password failed: {string.Join(", ", changed.Errors.Select(e => e.Description))}");
        }
        public async Task<string> GetRoleByUserIdAsync(Guid userId)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null) return string.Empty;
            var roles = await _userManager.GetRolesAsync(user);
            return roles.FirstOrDefault() ?? string.Empty;
        }

        private async Task<List<string>> GetPermissionsByUserIdAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return new();

            var roles = await _userManager.GetRolesAsync(user);
            var permissions = new List<string>();
            var allPermissions = new List<RoleClaimsDTO>();

            if (roles.Contains(Roles.Admin))
            {
                var types = typeof(Permissions).GetTypeInfo().DeclaredNestedTypes;
                foreach (var type in types) allPermissions.GetPermissions(type);
                permissions.AddRange(allPermissions.Select(p => p.Value));
            }
            else
            {
                foreach (var roleName in roles)
                {
                    var role = await _roleManager.FindByNameAsync(roleName);
                    if (role == null) continue;
                    var claims = await _roleManager.GetClaimsAsync(role);
                    permissions.AddRange(claims.Select(x => x.Value));
                }
            }

            return permissions.Distinct().ToList();
        }
    }
}