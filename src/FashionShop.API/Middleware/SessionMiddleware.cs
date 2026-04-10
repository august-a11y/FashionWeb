namespace FashionShop.API.Middleware
{
    public class SessionMiddleware
    {
        private readonly RequestDelegate _next;
        private const string SessionCookieName = "Session-Id";

        public SessionMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, Infrastructure.Services.RequestContext requestContext)
        {
            if (context.Request.Cookies.TryGetValue(SessionCookieName, out var sessionId)
                && !string.IsNullOrWhiteSpace(sessionId))
            {
                requestContext.SessionId = sessionId;
            }
            else
            {
                var newSessionId = Guid.NewGuid().ToString();
                requestContext.SessionId = newSessionId;

                context.Response.Cookies.Append(SessionCookieName, newSessionId, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.None,
                    MaxAge = TimeSpan.FromDays(30),
                    Path = "/"
                });
            }

            await _next(context);
        }
    }
}