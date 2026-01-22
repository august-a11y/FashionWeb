using FashionShop.Application.Common.Interfaces;
using FashionShop.Domain.Common;
using FashionShop.Domain.Entities;
using FashionShop.Domain.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace FashionShop.Infrastructure.Persistence
{
    public class ApplicationDbContext : IdentityDbContext<AppUser, AppRole, Guid>, IApplicationDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

       
            public DbSet<Product> Products { get; set; }
            public DbSet<ProductVariant> ProductVariants { get; set; }
            public DbSet<Category> Categories { get; set; }
            public DbSet<Order> Orders { get; set; }
            public DbSet<OrderItem> OrderItems { get; set; }
            public DbSet<AppRole> AppRoles { get; set; }
            public DbSet<AppUser> AppUsers { get; set; }
            public DbSet<Cart> Carts { get; set; }
            public DbSet<CartItem> CartItems { get; set; }
            public DbSet<Address> Address { get; set; }
 





        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // --- CẤU HÌNH IDENTITY THỦ CÔNG ---

            // 1. Cấu hình bảng User
            modelBuilder.Entity<AppUser>(b =>
            {
                b.ToTable("AppUsers");
                // Mỗi User có nhiều UserClaims
                b.HasMany(e => e.Claims)
                    .WithOne()
                    .HasForeignKey(uc => uc.UserId)
                    .IsRequired();

                // Mỗi User có nhiều UserLogins
                b.HasMany(e => e.Logins)
                    .WithOne()
                    .HasForeignKey(ul => ul.UserId)
                    .IsRequired();

                // Mỗi User có nhiều UserTokens
                b.HasMany(e => e.Tokens)
                    .WithOne()
                    .HasForeignKey(ut => ut.UserId)
                    .IsRequired();

                // Quan trọng: Mapping User với Role thông qua bảng trung gian UserRoles
                b.HasMany(e => e.UserRoles)
                    .WithOne()
                    .HasForeignKey(ur => ur.UserId) // Quan trọng: Chỉ định dùng cột UserId
                    .IsRequired()
                    .OnDelete(DeleteBehavior.NoAction);
            });

            // 2. Cấu hình bảng Role
            modelBuilder.Entity<AppRole>(b =>
            {
                b.ToTable("AppRoles");

                // Mỗi Role có nhiều UserRoles
                b.HasMany(e => e.UserRoles)
                    .WithOne()
                    .HasForeignKey(ur => ur.RoleId) // Quan trọng: Chỉ định dùng cột RoleId
                    .IsRequired()
                    .OnDelete(DeleteBehavior.NoAction);

                // Mỗi Role có nhiều RoleClaims
                b.HasMany(e => e.RoleClaims)
                    .WithOne()
                    .HasForeignKey(rc => rc.RoleId)
                    .IsRequired();
            });
            

           
           

            // 3. Cấu hình các bảng còn lại và đổi tên
            modelBuilder.Entity<IdentityUserClaim<Guid>>().ToTable("AppUserClaims");
            modelBuilder.Entity<IdentityRoleClaim<Guid>>().ToTable("AppRoleClaims");
            modelBuilder.Entity<IdentityUserLogin<Guid>>().ToTable("AppUserLogins").HasKey(x => new { x.LoginProvider, x.ProviderKey });
            modelBuilder.Entity<IdentityUserToken<Guid>>().ToTable("AppUserTokens").HasKey(x => new { x.UserId, x.LoginProvider, x.Name });

            // 4. Cấu hình bảng trung gian UserRole (Nơi hay mất liên kết nhất)
            modelBuilder.Entity<IdentityUserRole<Guid>>(b =>
            {
                b.ToTable("AppUserRoles");
                b.HasKey(x => new { x.UserId, x.RoleId }); // Set PK

                // Thiết lập FK trỏ về AppUser (Bắt buộc)
                b.HasOne<AppUser>()
                    .WithMany(e => e.UserRoles) // Hoặc .WithMany() nếu AppUser không có prop UserRoles
                    .HasForeignKey(x => x.UserId)
                    .IsRequired(); // FK Not Null

                // Thiết lập FK trỏ về AppRole (Bắt buộc)
                b.HasOne<AppRole>()
                    .WithMany(e => e.UserRoles) // Hoặc .WithMany() nếu AppRole không có prop UserRoles
                    .HasForeignKey(x => x.RoleId)
                    .IsRequired();
            });

            // --- CẤU HÌNH GLOBAL FILTER (SOFT DELETE) ---
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                if (typeof(ISoftDelete).IsAssignableFrom(entityType.ClrType))
                {
                    modelBuilder.Entity(entityType.ClrType).HasQueryFilter(GetIsDeletedFilter(entityType.ClrType));
                }
            }
        }
        protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
        {
            
            configurationBuilder.Properties<decimal>()
                .HavePrecision(18, 2);
        }
        public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken )
        {
            var entries = ChangeTracker.Entries().Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

            foreach (var entry in entries)
            {
                var dateCreatedProp = entry.Entity.GetType().GetProperty("CreatedAt");
                if (entry.State == EntityState.Added && dateCreatedProp != null)
                {
                    dateCreatedProp.SetValue(entry.Entity, DateTime.UtcNow);
                }
                var dateUpdatedProp = entry.Entity.GetType().GetProperty("UpdatedAt");
                if (entry.State == EntityState.Modified && dateUpdatedProp != null)
                {
                    dateUpdatedProp.SetValue(entry.Entity, DateTime.UtcNow);
                }
            }
            return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        }
        private LambdaExpression? GetIsDeletedFilter(Type type)
        {
            var param = Expression.Parameter(type, "x");
            var prop = Expression.Property(param, nameof(ISoftDelete.IsDeleted));
            var value = Expression.Constant(false);
            var body = Expression.Equal(prop, value); // x.IsDeleted == false
            return Expression.Lambda(body, param);
        }
    }
}
