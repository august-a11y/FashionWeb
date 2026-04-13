namespace FashionShop.Application.Services.AuthServices.Models
{
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
