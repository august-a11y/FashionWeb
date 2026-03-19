using FashionShop.Application.AuthServices.Models;
using FashionShop.Application.Common.Interfaces;
using FashionShop.Application.Extensions;
using FashionShop.Domain.Constants;
using FashionShop.Domain.Identity;
using FluentResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore.Storage.Json;
using Microsoft.IdentityModel.JsonWebTokens;
using System.Management;
using System.Reflection;
using System.Security.Claims;
using System.Text.Json;

namespace FashionShop.Application.AuthServices
{
    public class AuthenticatedService : IAuthenticatedService
    {
        private readonly IJwtService _jwtService;
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly RoleManager<AppRole> _roleManager;

        public AuthenticatedService(IJwtService jwtService, UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, RoleManager<AppRole> roleManager)
        {
            _userManager = userManager;
            _jwtService = jwtService;
            _signInManager = signInManager;
            _roleManager = roleManager;
        }
        public async Task<Result<AuthenticatedResponse>> LoginAsync(string username, string password)
        {
            var user = await _userManager.FindByNameAsync(username);
            if (user == null)
            {
                return Result.Fail("Invalid username or password.");
            }

            if (!user.IsActive)
            {
                return Result.Fail("User account is inactive.");
            }

            var result = await _signInManager.PasswordSignInAsync(
                user,
                password,
                isPersistent: false,
                lockoutOnFailure: true);

            if (result.IsLockedOut)
            {
                var lockoutEnd = await _userManager.GetLockoutEndDateAsync(user);
                var remaining = lockoutEnd.HasValue
                    ? lockoutEnd.Value - DateTimeOffset.UtcNow
                    : TimeSpan.Zero;

                return Result.Fail($"User account is locked. Try again in {Math.Max(0, remaining.Minutes)} minutes and {Math.Max(0, remaining.Seconds)} seconds.");
            }

            if (!result.Succeeded)
            {
                return Result.Fail("Invalid username or password.");
            }

            var roles = await _userManager.GetRolesAsync(user);
            var permissions = await GetPermissionsByUserIdAsync(user.Id.ToString());

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
                new Claim(UserClaims.FirstName, user.FirstName ?? string.Empty),
                new Claim(UserClaims.Id, user.Id.ToString()),
                new Claim(UserClaims.Roles, string.Join(";", roles)),
                new Claim(ClaimTypes.Name, user.UserName ?? string.Empty),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(UserClaims.Permissions, JsonSerializer.Serialize(permissions)),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var token = _jwtService.GenerateToken(claims);
            var refreshToken = _jwtService.GenerateRefreshToken();

            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
            await _userManager.UpdateAsync(user);

            return new AuthenticatedResponse
            {
                AccessToken = token,
                RefreshToken = refreshToken
            };
        }

        public async Task<Result<bool>> RegisterAsync(string username, string email, string firstName, string lastName, string phoneNumber, string password)
        {
            var userExists = await _userManager.FindByNameAsync(username);

            if (userExists != null)
            {
                return Result.Fail("User already exists!");
            }

            var emailExists = await _userManager.FindByEmailAsync(email);
            if (emailExists != null)
            {
                return Result.Fail("Email already exists!");
            }


            var user = new AppUser
            {
                Email = email,
                NormalizedEmail = email!.ToUpper(),
                SecurityStamp = Guid.NewGuid().ToString(),
                UserName = username,
                NormalizedUserName = username.ToUpper(),
                FirstName = firstName,
                LastName = lastName,
                PhoneNumber = phoneNumber
            };
            var result = await _userManager.CreateAsync(user, password);

            if (!result.Succeeded)
            {
                string errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return Result.Fail($"Register Fail: {errors}");
            }
            await _userManager.AddToRoleAsync(user, "User");
            return true;
        }

        private async Task<List<string>> GetPermissionsByUserIdAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return new List<string>();
            }
            var roles = await _userManager.GetRolesAsync(user);
            var permissions = new List<string>();

            var allPermisssions = new List<RoleClaimsDTO>();

            if (roles.Contains(Roles.Admin))
            {
                var types = typeof(Permissions).GetTypeInfo().DeclaredNestedTypes;
                foreach (var type in types)
                {
                    allPermisssions.GetPermissions(type);
                }
                permissions.AddRange(allPermisssions.Select(p => p.Value));
            }
            else
            {
                foreach (var roleName in roles)
                {
                    var role = await _roleManager.FindByNameAsync(roleName);
                    var claims = await _roleManager.GetClaimsAsync(role);
                    var roleClaimsValues = claims.Select(x => x.Value).ToList();
                    permissions.AddRange(roleClaimsValues);
                }
            }
            return permissions.Distinct().ToList();
        }
    }
}
