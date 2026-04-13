using FashionShop.Domain.Entities;
using FashionShop.Domain.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Infrastructure;


namespace FashionShop.Infrastructure.Interfaces
{
    public interface IApplicationDbContext
    {
        public DbSet<Product> Products { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<AppRole> AppRoles { get; set; }
        public DbSet<AppUser> AppUsers { get; set; }
        public DbSet<Cart> Carts { get; set; }
        public DbSet<CartItem> CartItems { get; set; }
        public DbSet<Variant> ProductVariants { get; set; }
        public DbSet<Address> Address { get; set; }
        public DatabaseFacade Database { get; }
        public ChangeTracker ChangeTracker { get; }
        Task<int> SaveChangesAsync(CancellationToken cancellationToken);
    }
}
