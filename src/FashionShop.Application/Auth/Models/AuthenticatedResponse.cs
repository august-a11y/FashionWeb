namespace FashionShop.Application.Auth.Models
{
    public record AuthenticatedResponse
    {
        public string AccessToken { get; init; } = string.Empty;
        public string RefreshToken { get; init; } = string.Empty;
    }
}
