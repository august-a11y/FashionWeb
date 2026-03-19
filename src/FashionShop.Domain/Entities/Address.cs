using FashionShop.Domain.Common;
using FashionShop.Domain.Identity;

namespace FashionShop.Domain.Entities
{
    public class Address : BaseEntity
    {
        public Guid UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string StreetLine { get; set; } = string.Empty;
        public string Ward { get; set; } = string.Empty;
        public string District { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public virtual AppUser User { get; set; }
    }
}
