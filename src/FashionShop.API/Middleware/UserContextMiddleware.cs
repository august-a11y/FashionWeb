using FashionShop.Domain.Common.Interfaces;
using FashionShop.Infrastructure.Services;

namespace FashionShop.API.Middleware
{
    public class UserContextMiddleware
    {
        private readonly RequestDelegate _next;

        public UserContextMiddleware(RequestDelegate next)
        {
            
            _next = next;
        }
        public async Task InvokeAsync(HttpContext context, UserContext userContext)
        {
            if(context.Request.Headers.TryGetValue("X-Guest-Id", out var sessionId))
            {
                userContext.SessionId = sessionId.ToString();
            }
            else
            {
                throw new Exception("X-Guest-Id header is missing");
            }
            if(context.User.Identity!.IsAuthenticated == true)
            {
                userContext.IsAuthenticated = true;
                userContext.UserId = context.User.Claims.FirstOrDefault(c => c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value is string userIdStr && Guid.TryParse(userIdStr, out var userId)
                    ? userId
                    : null;

            }

                await _next(context);
        }
    }
}
