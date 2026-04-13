namespace FashionShop.Application.Services.CartServices.Helper
{
    public static class CartKeyHelper
    {
        public static string GetCartKey(Guid? userId, string? guestId = null)
        {
            if (userId != null)
                return $"cart:user:{userId}";
            return $"cart:guest:{guestId}";
        }

        public static string GetItemFieldKey(Guid productId, Guid? variantId)
        {
            return $"{productId}-{variantId}";
        }
    }
}
