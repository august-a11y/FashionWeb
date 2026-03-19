using FashionShop.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace FashionShop.Domain.Identity
{
    public class AppUser : IdentityUser<Guid>
    {

        public string FirstName { get; set; } = string.Empty;   

        public string LastName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
        public string RefreshToken { get; set; } = string.Empty;
        public DateTime? RefreshTokenExpiryTime { get; set; }
        public bool IsActive { get; set; } = true;
        public bool IsDeleted { get; set; } = false;
        public virtual ICollection<Address> Addresses { get; set; }
        public virtual Cart Cart { get; set; }
        public virtual ICollection<Order> Orders { get; set; }
        public virtual  ICollection<IdentityUserRole<Guid>> UserRoles { get; set; }
        public virtual  ICollection<IdentityUserClaim<Guid>> Claims { get; set; }
        public virtual  ICollection<IdentityUserLogin<Guid>> Logins { get; set; }
        public virtual  ICollection<IdentityUserToken<Guid>> Tokens { get; set; }

    }
}
