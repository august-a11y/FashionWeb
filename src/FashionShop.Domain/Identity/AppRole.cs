using Microsoft.AspNetCore.Identity;

namespace FashionShop.Domain.Identity
{
    public class AppRole : IdentityRole<Guid>
    {
        public string DisplayName { get; set; } = string.Empty;
        public DateTime? CreatedAt { get; set; } 
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
        
        public bool IsDeleted { get; set; } = false;
        public virtual ICollection<IdentityUserRole<Guid>> UserRoles { get; set; }
        public virtual ICollection<IdentityRoleClaim<Guid>> RoleClaims { get; set; }
    }
}
