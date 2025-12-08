

using FashionShop.Domain.Common;

namespace FashionShop.Domain.Entities
{
    public class TryOnHistory : BaseEntity
    {
        public string UserId { get; set; } = string.Empty;

        public int ProductId { get; set; }
        public Product? Product { get; set; }

        public string UserOriginalImageUrl { get; set; } = string.Empty;

        public string GeneratedImageUrl { get; set; } = string.Empty;

        public TryOnStatus Status { get; set; } = TryOnStatus.Pending;

    }

    public enum TryOnStatus
    {
        Pending,
        Success,
        Failed
    }
}
