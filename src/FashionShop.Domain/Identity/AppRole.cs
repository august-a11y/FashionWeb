using FashionShop.Domain.Common;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FashionShop.Domain.Identity
{
    public class AppRole : IdentityRole<Guid>
    {
        [Required]
        [MaxLength(256)]
        public string DisplayName { get; set; } = string.Empty;
        public DateTime? CreatedAt { get; set; } 
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
        
        public bool IsDeleted { get; set; } = false;
        public virtual ICollection<IdentityUserRole<Guid>> UserRoles { get; set; }
        public virtual ICollection<IdentityRoleClaim<Guid>> RoleClaims { get; set; }
    }
}
