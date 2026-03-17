using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using FashionShop.Infrastructure.ConfigOptions;
using FashionShop.Infrastructure.Services;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace FashionShop.API.Middleware
{
    public class JwtContextMiddleware
    {
        private readonly RequestDelegate _next;

        public JwtContextMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, RequestContext requestContext, IOptions<JwtTokenSettings> jwtOptions)
        {
            var authHeader = context.Request.Headers.Authorization.FirstOrDefault();

            if (!string.IsNullOrWhiteSpace(authHeader) && authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                var token = authHeader["Bearer ".Length..].Trim();

                var principal = ValidateToken(token, jwtOptions.Value);
                if (principal != null)
                {
                    // Set HttpContext.User so [Authorize] and policies work correctly
                    context.User = principal;

                    requestContext.IsAuthenticated = true;
                    requestContext.UserId = principal.Claims
                        .FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier
                                          || c.Type == JwtRegisteredClaimNames.Sub)
                        ?.Value is string userIdStr && Guid.TryParse(userIdStr, out var userId)
                        ? userId
                        : null;
                }
            }

            await _next(context);
        }

        private static ClaimsPrincipal? ValidateToken(string token, JwtTokenSettings settings)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.UTF8.GetBytes(settings.Key);

                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = settings.Issuer,
                    ValidAudience = settings.Issuer,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ClockSkew = TimeSpan.FromMinutes(1)
                };

                var principal = tokenHandler.ValidateToken(token, validationParameters, out var securityToken);

                if (securityToken is not JwtSecurityToken jwtToken
                    || !jwtToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.OrdinalIgnoreCase))
                {
                    return null;
                }

                return principal;
            }
            catch
            {
                // Token invalid or expired — treat as unauthenticated
                return null;
            }
        }
    }
}