using FashionShop.Domain.Entities;
using FashionShop.Domain.Identity;
using FashionShop.Domain.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FashionShop.Infrastructure.Persistence
{
    public class DataSeeder
    {
        
        public async Task SeedAsync(ApplicationDbContext context)
        {
            var passwordHasher = new PasswordHasher<AppUser>();

            // Định nghĩa ID cứng để dễ truy vấn lại nếu cần
            var rootAdminRoleId = Guid.Parse("11111111-1111-1111-1111-111111111111");
            var customerRoleId = Guid.Parse("22222222-2222-2222-2222-222222222222");

            if (!context.Roles.Any())
            {
                await context.Roles.AddRangeAsync(
                    new AppRole
                    {
                        Id = rootAdminRoleId,
                        Name = "Admin",
                        NormalizedName = "ADMIN",
                        DisplayName = "Administrator"
                    },
                    new AppRole
                    {
                        Id = customerRoleId,
                        Name = "Customer", // Đã sửa lỗi chính tả Custommer -> Customer
                        NormalizedName = "CUSTOMER",
                        DisplayName = "Customer"
                    }
                );
                await context.SaveChangesAsync();
            }

            if (!context.Users.Any())
            {
                // Admin User
                var adminId = Guid.NewGuid();
                var adminUser = new AppUser
                {
                    Id = adminId,
                    FirstName = "Le",
                    LastName = "Xuan Vy",
                    UserName = "lexuanvy2006",
                    NormalizedUserName = "LEXUANVY2006",
                    Email = "xuanvyhv572@gmail.com",
                    NormalizedEmail = "XUANVYHV572@GMAIL.COM",
                    IsActive = true,
                    SecurityStamp = Guid.NewGuid().ToString(),
                    LockoutEnabled = false,
                    CreatedAt = DateTime.UtcNow,
                };
                adminUser.PasswordHash = passwordHasher.HashPassword(adminUser, "Vy@2006");
                await context.Users.AddAsync(adminUser);
                await context.UserRoles.AddAsync(new IdentityUserRole<Guid> { RoleId = rootAdminRoleId, UserId = adminId });

                // Customer User
                var customerId = Guid.NewGuid();
                var customerUser = new AppUser
                {
                    Id = customerId,
                    FirstName = "Le",
                    LastName = "Thanh Tuan",
                    UserName = "lethanhtuan2001",
                    NormalizedUserName = "LETHANHTUAN2001",
                    Email = "thanhtuanroby@gmail.com",
                    NormalizedEmail = "THANHTUANROBY@GMAIL.COM",
                    IsActive = true,
                    SecurityStamp = Guid.NewGuid().ToString(),
                    LockoutEnabled = false,
                    CreatedAt = DateTime.UtcNow,
                };
                customerUser.PasswordHash = passwordHasher.HashPassword(customerUser, "Tuan@2001");
                await context.Users.AddAsync(customerUser);
                await context.UserRoles.AddAsync(new IdentityUserRole<Guid> { RoleId = customerRoleId, UserId = customerId });

                await context.SaveChangesAsync();
            }

            // 2. SEED CATEGORIES
            if (!await context.Categories.AnyAsync())
            {
                context.Categories.AddRange(
                    new Category (  "Áo", "Các loại áo thời trang"),
                    new Category ("Quần", "Các loại quần thời trang")
                );
                await context.SaveChangesAsync();
            }

            // Lấy ID của Category từ DB (để đảm bảo có ID dù vừa seed hay đã có từ trước)
            var shirtCategory = await context.Categories.FirstOrDefaultAsync(c => c.Name == "Áo");
            var pantCategory = await context.Categories.FirstOrDefaultAsync(c => c.Name == "Quần");

            // 3. SEED PRODUCTS & VARIANTS
            if (!await context.Products.AnyAsync())
            {
                // --- Sản phẩm 1: Áo thun ---
                var shirtProduct = new Product
                {
                    Id = Guid.NewGuid(),
                    Name = "Áo thun basic",
                    Description = "Áo thun cotton thoáng mát",
                    BasePrice = 199000,
                    CategoryId = shirtCategory.Id,
                    ThumbnailUrl = "/images/products/ao-thun-basic.jpg", // Link ảnh đại diện
                    CreatedAt = DateTime.UtcNow,
                    IsDeleted = false
                };

                // --- Sản phẩm 2: Quần Jeans ---
                var jeanProduct = new Product
                {
                    Id = Guid.NewGuid(),
                    Name = "Quần jeans slimfit",
                    Description = "Quần jeans co giãn",
                    BasePrice = 399000,
                    CategoryId = pantCategory.Id,
                    ThumbnailUrl = "/images/products/quan-jeans.jpg",
                    CreatedAt = DateTime.UtcNow,
                    IsDeleted = false
                };

                context.Products.AddRange(shirtProduct, jeanProduct);
                await context.SaveChangesAsync(); // Lưu Product trước để có ID dùng cho Variant

                // 4. SEED VARIANTS (QUAN TRỌNG VỚI DB MỚI)
                // Tạo biến thể cho Áo thun (Màu Trắng/Đen - Size M/L)
                var shirtVariants = new List<ProductVariant>
                {
                    new ProductVariant
                    {
                        ProductId = shirtProduct.Id,
                        Sku = "AT-BASIC-W-M",
                        Size = "M",
                        Color = "Trắng",
                        Price = 199000,
                        StockQuantity = 50,
                        ThumbnailUrl = "/images/products/ao-thun-trang.jpg",
                        CreatedAt = DateTime.UtcNow
                    },
                    new ProductVariant
                    {
                        ProductId = shirtProduct.Id,
                        Sku = "AT-BASIC-W-L",
                        Size = "L",
                        Color = "Trắng",
                        Price = 199000,
                        StockQuantity = 30,
                        ThumbnailUrl = "/images/products/ao-thun-trang.jpg",
                        CreatedAt = DateTime.UtcNow
                    },
                    new ProductVariant
                    {
                        ProductId = shirtProduct.Id,
                        Sku = "AT-BASIC-B-M",
                        Size = "M",
                        Color = "Đen",
                        Price = 209000, // Ví dụ màu đen đắt hơn chút
                        StockQuantity = 20,
                        ThumbnailUrl = "/images/products/ao-thun-den.jpg",
                        CreatedAt = DateTime.UtcNow
                    }
                };

                // Tạo biến thể cho Quần Jeans (Size 29/30/31)
                var jeanVariants = new List<ProductVariant>
                {
                    new ProductVariant
                    {
                        ProductId = jeanProduct.Id,
                        Sku = "QJ-SLIM-29",
                        Size = "29",
                        Color = "Xanh Navy",
                        Price = 399000,
                        StockQuantity = 100,
                        ThumbnailUrl = "/images/products/quan-jeans-navy.jpg",
                        CreatedAt = DateTime.UtcNow
                    },
                    new ProductVariant
                    {
                        ProductId = jeanProduct.Id,
                        Sku = "QJ-SLIM-30",
                        Size = "30",
                        Color = "Xanh Navy",
                        Price = 399000,
                        StockQuantity = 80,
                        ThumbnailUrl = "/images/products/quan-jeans-navy.jpg",
                        CreatedAt = DateTime.UtcNow
                    }
                };

                context.ProductVariants.AddRange(shirtVariants);
                context.ProductVariants.AddRange(jeanVariants);

                await context.SaveChangesAsync();
            }
        }
    }
}
