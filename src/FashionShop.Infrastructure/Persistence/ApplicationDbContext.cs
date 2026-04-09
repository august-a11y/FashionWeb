using FashionShop.Application.Common.Interfaces;
using FashionShop.Domain.Common;
using FashionShop.Domain.Entities;
using FashionShop.Domain.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace FashionShop.Infrastructure.Persistence
{
    public class ApplicationDbContext : IdentityDbContext<AppUser, AppRole, Guid>, IApplicationDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

       
            public DbSet<Product> Products { get; set; }
            public DbSet<Variant> ProductVariants { get; set; }
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

        
            modelBuilder.Entity<AppUser>(b =>
            {
                b.ToTable("AppUsers");
           
                b.HasMany(e => e.Claims)
                    .WithOne()
                    .HasForeignKey(uc => uc.UserId)
                    .IsRequired();

                b.HasMany(e => e.Logins)
                    .WithOne()
                    .HasForeignKey(ul => ul.UserId)
                    .IsRequired();

      
                b.HasMany(e => e.Tokens)
                    .WithOne()
                    .HasForeignKey(ut => ut.UserId)
                    .IsRequired();
                b.HasIndex(u => u.Email)
                    .IsUnique();
                b.HasOne(e =>e.Cart)
                    .WithOne(c => c.User)
                    .HasForeignKey<Cart>(c => c.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                b.HasMany(e => e.UserRoles)
                    .WithOne()
                    .HasForeignKey(ur => ur.UserId) 
                    .IsRequired()
                    .OnDelete(DeleteBehavior.NoAction);
            });

            
            modelBuilder.Entity<AppRole>(b =>
            {
                b.ToTable("AppRoles");

           
                b.HasMany(e => e.UserRoles)
                    .WithOne()
                    .HasForeignKey(ur => ur.RoleId) 
                    .IsRequired()
                    .OnDelete(DeleteBehavior.NoAction);

                b.HasMany(e => e.RoleClaims)
                    .WithOne()
                    .HasForeignKey(rc => rc.RoleId)
                    .IsRequired();
            });

            modelBuilder.Entity<CartItem>()
                .HasOne(c => c.Product) 
                .WithMany()
                .HasForeignKey(c => c.ProductId)
                .OnDelete(DeleteBehavior.Restrict); 

        
            modelBuilder.Entity<CartItem>()
                .HasOne(c => c.Variant) 
                .WithMany()
                .HasForeignKey(c => c.VariantId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Address>()
            .HasOne(a => a.User)
            .WithMany(u => u.Addresses)
            .HasForeignKey(a => a.UserId);

       
            modelBuilder.Entity<Order>()
                .HasOne(o => o.User)
                .WithMany(u => u.Orders)
                .HasForeignKey(o => o.UserId);


            modelBuilder.Entity<Category>()
                .HasOne(c => c.ParentCategory)
                .WithMany(c => c.Children)
                .HasForeignKey(c => c.ParentCategoryId)
                .OnDelete(DeleteBehavior.Restrict);
  


  
            modelBuilder.Entity<IdentityUserClaim<Guid>>().ToTable("AppUserClaims");
            modelBuilder.Entity<IdentityRoleClaim<Guid>>().ToTable("AppRoleClaims");
            modelBuilder.Entity<IdentityUserLogin<Guid>>().ToTable("AppUserLogins").HasKey(x => new { x.LoginProvider, x.ProviderKey });
            modelBuilder.Entity<IdentityUserToken<Guid>>().ToTable("AppUserTokens").HasKey(x => new { x.UserId, x.LoginProvider, x.Name });

          
            modelBuilder.Entity<IdentityUserRole<Guid>>(b =>
            {
                b.ToTable("AppUserRoles");
                b.HasKey(x => new { x.UserId, x.RoleId }); 

              
                b.HasOne<AppUser>()
                    .WithMany(e => e.UserRoles) 
                    .HasForeignKey(x => x.UserId)
                    .IsRequired(); 

                
                b.HasOne<AppRole>()
                    .WithMany(e => e.UserRoles) 
                    .HasForeignKey(x => x.RoleId)
                    .IsRequired();
            });

            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

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
            var body = Expression.Equal(prop, value); 
            return Expression.Lambda(body, param);
        }
    }
}
