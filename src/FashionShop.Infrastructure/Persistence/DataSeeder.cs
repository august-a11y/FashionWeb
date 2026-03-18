using FashionShop.Domain.Constants;
using FashionShop.Domain.Entities;
using FashionShop.Domain.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace FashionShop.Infrastructure.Persistence
{
    public class DataSeeder
    {
        private static readonly Guid AdminRoleId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        private static readonly Guid UserRoleId = Guid.Parse("22222222-2222-2222-2222-222222222222");

        private static readonly Guid AdminUserId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
        private static readonly Guid DemoUserId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");

        private const string DemoOrderCode = "DEMO-ORDER-0001";

        public async Task SeedAsync(ApplicationDbContext context, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(context);

            var passwordHasher = new PasswordHasher<AppUser>();

            await SeedRolesAsync(context, cancellationToken);
            await SeedUsersAsync(context, passwordHasher, cancellationToken);
            await SeedAddressesAsync(context, cancellationToken);
            await SeedCatalogAsync(context, cancellationToken);
            await SeedCartsAsync(context, cancellationToken);
            await SeedOrdersAsync(context, cancellationToken);
        }

        private static async Task SeedRolesAsync(ApplicationDbContext context, CancellationToken cancellationToken)
        {
            var requiredRoles = new List<AppRole>
            {
                new()
                {
                    Id = AdminRoleId,
                    Name = Roles.Admin,
                    NormalizedName = Roles.Admin.ToUpperInvariant(),
                    DisplayName = "Administrator",
                    IsDeleted = false
                },
                new()
                {
                    Id = UserRoleId,
                    Name = "User",
                    NormalizedName = "USER",
                    DisplayName = "User",
                    IsDeleted = false
                }
            };

            var existingRoles = await context.Roles
                .IgnoreQueryFilters()
                .ToListAsync(cancellationToken);

            var hasChanges = false;

            foreach (var role in requiredRoles)
            {
                var existing = existingRoles.FirstOrDefault(r => r.NormalizedName == role.NormalizedName);
                if (existing is null)
                {
                    await context.Roles.AddAsync(role, cancellationToken);
                    hasChanges = true;
                    continue;
                }

                var changed = false;
                if (!string.Equals(existing.Name, role.Name, StringComparison.Ordinal))
                {
                    existing.Name = role.Name;
                    changed = true;
                }

                if (!string.Equals(existing.DisplayName, role.DisplayName, StringComparison.Ordinal))
                {
                    existing.DisplayName = role.DisplayName;
                    changed = true;
                }

                if (existing.IsDeleted)
                {
                    existing.IsDeleted = false;
                    existing.DeletedAt = null;
                    changed = true;
                }

                if (changed)
                {
                    context.Roles.Update(existing);
                    hasChanges = true;
                }
            }

            if (hasChanges)
            {
                await context.SaveChangesAsync(cancellationToken);
            }
        }

        private static async Task SeedUsersAsync(
            ApplicationDbContext context,
            PasswordHasher<AppUser> passwordHasher,
            CancellationToken cancellationToken)
        {
            var seedUsers = new[]
            {
                new
                {
                    Id = AdminUserId,
                    FirstName = "System",
                    LastName = "Admin",
                    UserName = "admin",
                    Email = "admin@fashionshop.local",
                    Password = "Admin@123",
                    RoleName = Roles.Admin
                },
                new
                {
                    Id = DemoUserId,
                    FirstName = "Demo",
                    LastName = "User",
                    UserName = "user",
                    Email = "user@fashionshop.local",
                    Password = "User@123",
                    RoleName = "User"
                }
            };

            foreach (var item in seedUsers)
            {
                var normalizedUserName = item.UserName.ToUpperInvariant();
                var user = await context.Users
                    .IgnoreQueryFilters()
                    .FirstOrDefaultAsync(u => u.NormalizedUserName == normalizedUserName, cancellationToken);

                if (user is null)
                {
                    user = new AppUser
                    {
                        Id = item.Id,
                        FirstName = item.FirstName,
                        LastName = item.LastName,
                        UserName = item.UserName,
                        NormalizedUserName = normalizedUserName,
                        Email = item.Email,
                        NormalizedEmail = item.Email.ToUpperInvariant(),
                        EmailConfirmed = true,
                        IsActive = true,
                        IsDeleted = false,
                        LockoutEnabled = true,
                        SecurityStamp = Guid.NewGuid().ToString()
                    };

                    user.PasswordHash = passwordHasher.HashPassword(user, item.Password);
                    await context.Users.AddAsync(user, cancellationToken);
                    await context.SaveChangesAsync(cancellationToken);
                }
                else
                {
                    var changed = false;

                    if (user.IsDeleted)
                    {
                        user.IsDeleted = false;
                        user.DeletedAt = null;
                        changed = true;
                    }

                    if (!user.IsActive)
                    {
                        user.IsActive = true;
                        changed = true;
                    }

                    if (changed)
                    {
                        context.Users.Update(user);
                        await context.SaveChangesAsync(cancellationToken);
                    }
                }

                var role = await context.Roles
                    .IgnoreQueryFilters()
                    .FirstOrDefaultAsync(r => r.NormalizedName == item.RoleName.ToUpperInvariant(), cancellationToken);

                if (role is null)
                {
                    continue;
                }

                var hasRole = await context.UserRoles
                    .AnyAsync(ur => ur.UserId == user.Id && ur.RoleId == role.Id, cancellationToken);

                if (!hasRole)
                {
                    await context.UserRoles.AddAsync(
                        new IdentityUserRole<Guid> { UserId = user.Id, RoleId = role.Id },
                        cancellationToken);
                    await context.SaveChangesAsync(cancellationToken);
                }
            }
        }

        private static async Task SeedAddressesAsync(ApplicationDbContext context, CancellationToken cancellationToken)
        {
            var seeds = new[]
            {
                new
                {
                    UserId = AdminUserId,
                    FullName = "System Admin",
                    PhoneNumber = "0900000001",
                    StreetLine = "1 Nguyen Hue",
                    Ward = "Ben Nghe",
                    District = "District 1",
                    City = "Ho Chi Minh"
                },
                new
                {
                    UserId = DemoUserId,
                    FullName = "Demo User",
                    PhoneNumber = "0900000002",
                    StreetLine = "10 Le Loi",
                    Ward = "Ben Thanh",
                    District = "District 1",
                    City = "Ho Chi Minh"
                }
            };

            foreach (var item in seeds)
            {
                var userExists = await context.Users
                    .IgnoreQueryFilters()
                    .AnyAsync(u => u.Id == item.UserId, cancellationToken);

                if (!userExists)
                {
                    continue;
                }

                var existing = await context.Address
                    .IgnoreQueryFilters()
                    .FirstOrDefaultAsync(a =>
                        a.UserId == item.UserId &&
                        a.StreetLine == item.StreetLine &&
                        a.Ward == item.Ward &&
                        a.District == item.District &&
                        a.City == item.City,
                        cancellationToken);

                if (existing is null)
                {
                    await context.Address.AddAsync(new Address
                    {
                        UserId = item.UserId,
                        FullName = item.FullName,
                        PhoneNumber = item.PhoneNumber,
                        StreetLine = item.StreetLine,
                        Ward = item.Ward,
                        District = item.District,
                        City = item.City,
                        IsDeleted = false
                    }, cancellationToken);

                    continue;
                }

                var changed = false;
                if (existing.IsDeleted)
                {
                    existing.IsDeleted = false;
                    existing.DeletedAt = null;
                    changed = true;
                }

                if (!string.Equals(existing.FullName, item.FullName, StringComparison.Ordinal))
                {
                    existing.FullName = item.FullName;
                    changed = true;
                }

                if (!string.Equals(existing.PhoneNumber, item.PhoneNumber, StringComparison.Ordinal))
                {
                    existing.PhoneNumber = item.PhoneNumber;
                    changed = true;
                }

                if (changed)
                {
                    context.Address.Update(existing);
                }
            }

            await context.SaveChangesAsync(cancellationToken);
        }

        private static async Task SeedCatalogAsync(ApplicationDbContext context, CancellationToken cancellationToken)
        {
            var categorySeeds = new[]
            {
                new { Name = "Áo", Description = "Các loại áo thời trang" },
                new { Name = "Quần", Description = "Các loại quần thời trang" }
            };

            foreach (var item in categorySeeds)
            {
                var category = await context.Categories
                    .IgnoreQueryFilters()
                    .FirstOrDefaultAsync(c => c.Name == item.Name, cancellationToken);

                if (category is null)
                {
                    await context.Categories.AddAsync(new Category(item.Name, item.Description), cancellationToken);
                    continue;
                }

                var changed = false;
                if (category.IsDeleted)
                {
                    category.IsDeleted = false;
                    category.DeletedAt = null;
                    changed = true;
                }

                if (!string.Equals(category.Description, item.Description, StringComparison.Ordinal))
                {
                    category.Description = item.Description;
                    changed = true;
                }

                if (changed)
                {
                    context.Categories.Update(category);
                }
            }

            await context.SaveChangesAsync(cancellationToken);

            var shirtCategoryId = await context.Categories
                .Where(c => c.Name == "Áo")
                .Select(c => c.Id)
                .FirstAsync(cancellationToken);

            var pantCategoryId = await context.Categories
                .Where(c => c.Name == "Quần")
                .Select(c => c.Id)
                .FirstAsync(cancellationToken);

            var productSeeds = new[]
            {
                new
                {
                    Name = "Áo thun basic",
                    Description = "Áo thun cotton thoáng mát",
                    BasePrice = 199000m,
                    CategoryId = shirtCategoryId,
                    ThumbnailUrl = "/images/products/ao-thun-basic.jpg"
                },
                new
                {
                    Name = "Quần jeans slimfit",
                    Description = "Quần jeans co giãn",
                    BasePrice = 399000m,
                    CategoryId = pantCategoryId,
                    ThumbnailUrl = "/images/products/quan-jeans.jpg"
                }
            };

            foreach (var item in productSeeds)
            {
                var product = await context.Products
                    .IgnoreQueryFilters()
                    .FirstOrDefaultAsync(p => p.Name == item.Name, cancellationToken);

                if (product is null)
                {
                    await context.Products.AddAsync(new Product
                    {
                        Name = item.Name,
                        Description = item.Description,
                        BasePrice = item.BasePrice,
                        CategoryId = item.CategoryId,
                        ThumbnailUrl = item.ThumbnailUrl,
                        IsDeleted = false
                    }, cancellationToken);
                    continue;
                }

                var changed = false;
                if (product.IsDeleted)
                {
                    product.IsDeleted = false;
                    product.DeletedAt = null;
                    changed = true;
                }

                if (!string.Equals(product.Description, item.Description, StringComparison.Ordinal))
                {
                    product.Description = item.Description;
                    changed = true;
                }

                if (product.BasePrice != item.BasePrice)
                {
                    product.BasePrice = item.BasePrice;
                    changed = true;
                }

                if (product.CategoryId != item.CategoryId)
                {
                    product.CategoryId = item.CategoryId;
                    changed = true;
                }

                if (!string.Equals(product.ThumbnailUrl, item.ThumbnailUrl, StringComparison.Ordinal))
                {
                    product.ThumbnailUrl = item.ThumbnailUrl;
                    changed = true;
                }

                if (changed)
                {
                    context.Products.Update(product);
                }
            }

            await context.SaveChangesAsync(cancellationToken);

            var shirtProductId = await context.Products
                .Where(p => p.Name == "Áo thun basic")
                .Select(p => p.Id)
                .FirstAsync(cancellationToken);

            var jeanProductId = await context.Products
                .Where(p => p.Name == "Quần jeans slimfit")
                .Select(p => p.Id)
                .FirstAsync(cancellationToken);

            var variantSeeds = new[]
            {
                new { ProductId = shirtProductId, Size = "M", Color = "Trắng", Price = 199000m, Stock = 50, Thumbnail = "/images/products/ao-thun-trang.jpg" },
                new { ProductId = shirtProductId, Size = "L", Color = "Trắng", Price = 199000m, Stock = 30, Thumbnail = "/images/products/ao-thun-trang.jpg" },
                new { ProductId = shirtProductId, Size = "M", Color = "Đen", Price = 209000m, Stock = 20, Thumbnail = "/images/products/ao-thun-den.jpg" },
                new { ProductId = jeanProductId, Size = "29", Color = "Xanh Navy", Price = 399000m, Stock = 100, Thumbnail = "/images/products/quan-jeans-navy.jpg" },
                new { ProductId = jeanProductId, Size = "30", Color = "Xanh Navy", Price = 399000m, Stock = 80, Thumbnail = "/images/products/quan-jeans-navy.jpg" }
            };

            foreach (var item in variantSeeds)
            {
                var variant = await context.ProductVariants
                    .IgnoreQueryFilters()
                    .FirstOrDefaultAsync(v =>
                        v.ProductId == item.ProductId &&
                        v.Size == item.Size &&
                        v.Color == item.Color,
                        cancellationToken);

                if (variant is null)
                {
                    await context.ProductVariants.AddAsync(new Variant
                    {
                        ProductId = item.ProductId,
                        Size = item.Size,
                        Color = item.Color,
                        Price = item.Price,
                        StockQuantity = item.Stock,
                        ThumbnailUrl = item.Thumbnail,
                        IsDeleted = false
                    }, cancellationToken);
                    continue;
                }

                var changed = false;
                if (variant.IsDeleted)
                {
                    variant.IsDeleted = false;
                    variant.DeletedAt = null;
                    changed = true;
                }

                if (variant.Price != item.Price)
                {
                    variant.Price = item.Price;
                    changed = true;
                }

                if (variant.StockQuantity != item.Stock)
                {
                    variant.StockQuantity = item.Stock;
                    changed = true;
                }

                if (!string.Equals(variant.ThumbnailUrl, item.Thumbnail, StringComparison.Ordinal))
                {
                    variant.ThumbnailUrl = item.Thumbnail;
                    changed = true;
                }

                if (changed)
                {
                    context.ProductVariants.Update(variant);
                }
            }

            await context.SaveChangesAsync(cancellationToken);
        }

        private static async Task SeedCartsAsync(ApplicationDbContext context, CancellationToken cancellationToken)
        {
            var demoUser = await context.Users
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(u => u.Id == DemoUserId, cancellationToken);

            if (demoUser is null)
            {
                return;
            }

            var firstVariant = await context.ProductVariants
                .IgnoreQueryFilters()
                .OrderBy(v => v.CreatedAt)
                .FirstOrDefaultAsync(cancellationToken);

            if (firstVariant is null)
            {
                return;
            }

            var cart = await context.Carts
                .IgnoreQueryFilters()
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.UserId == demoUser.Id, cancellationToken);

            if (cart is null)
            {
                cart = new Cart(demoUser.Id)
                {
                    IsDeleted = false
                };
                await context.Carts.AddAsync(cart, cancellationToken);
                await context.SaveChangesAsync(cancellationToken);
            }
            else if (cart.IsDeleted)
            {
                cart.IsDeleted = false;
                cart.DeletedAt = null;
                context.Carts.Update(cart);
                await context.SaveChangesAsync(cancellationToken);
            }

            var existingItem = cart.Items.FirstOrDefault(i =>
                i.ProductId == firstVariant.ProductId &&
                i.VariantId == firstVariant.Id);

            if (existingItem is null)
            {
                await context.CartItems.AddAsync(new CartItem
                {
                    CartId = cart.Id,
                    ProductId = firstVariant.ProductId,
                    VariantId = firstVariant.Id,
                    Quantity = 2,
                    IsDeleted = false
                }, cancellationToken);
            }
            else
            {
                var changed = false;
                if (existingItem.IsDeleted)
                {
                    existingItem.IsDeleted = false;
                    existingItem.DeletedAt = null;
                    changed = true;
                }

                if (existingItem.Quantity != 2)
                {
                    existingItem.Quantity = 2;
                    changed = true;
                }

                if (changed)
                {
                    context.CartItems.Update(existingItem);
                }
            }

            cart.LastUpdate = DateTime.UtcNow;
            context.Carts.Update(cart);

            await context.SaveChangesAsync(cancellationToken);
        }

        private static async Task SeedOrdersAsync(ApplicationDbContext context, CancellationToken cancellationToken)
        {
            var demoUser = await context.Users
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(u => u.Id == DemoUserId, cancellationToken);

            if (demoUser is null)
            {
                return;
            }

            var shippingAddress = await context.Address
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(a => a.UserId == demoUser.Id, cancellationToken);

            if (shippingAddress is null)
            {
                return;
            }

            var selectedVariants = await context.ProductVariants
                .IgnoreQueryFilters()
                .Include(v => v.Product)
                .Where(v => !v.IsDeleted)
                .OrderBy(v => v.CreatedAt)
                .Take(2)
                .ToListAsync(cancellationToken);

            if (selectedVariants.Count == 0)
            {
                return;
            }

            var order = await context.Orders
                .IgnoreQueryFilters()
                .Include(o => o.OrderItems)
                .Include(o => o.Payment)
                .Include(o => o.Shipment)
                .FirstOrDefaultAsync(o => o.OrderCode == DemoOrderCode, cancellationToken);

            var orderItemSeeds = selectedVariants.Select((variant, index) => new
            {
                Variant = variant,
                Quantity = index == 0 ? 1 : 2
            }).ToList();

            var subTotal = orderItemSeeds.Sum(x => x.Variant.Price * x.Quantity);
            var shippingFee = 30000m;
            var total = subTotal + shippingFee;

            if (order is null)
            {
                order = new Order
                {
                    OrderCode = DemoOrderCode,
                    UserId = demoUser.Id,
                    PhoneNumber = shippingAddress.PhoneNumber,
                    Note = "Đơn hàng demo seed tự động",
                    Status = OrderStatus.Processing,
                    ExpectedDeliveryDate = DateTime.UtcNow.AddDays(3),
                    ShippingAddress = new OrderAddressSnapshot
                    {
                        RecipientName = shippingAddress.FullName,
                        RecipientPhone = shippingAddress.PhoneNumber,
                        StreetAddress = shippingAddress.StreetLine,
                        Ward = shippingAddress.Ward,
                        District = shippingAddress.District,
                        City = shippingAddress.City
                    },
                    SubTotal = subTotal,
                    ShippingFee = shippingFee,
                    TotalAmount = total,
                    IsDeleted = false
                };

                foreach (var item in orderItemSeeds)
                {
                    order.OrderItems.Add(new OrderItem
                    {
                        ProductId = item.Variant.ProductId,
                        VariantId = item.Variant.Id,
                        ProductName = item.Variant.Product?.Name ?? string.Empty,
                        Color = item.Variant.Color,
                        Size = item.Variant.Size,
                        ThumbnailUrl = item.Variant.ThumbnailUrl,
                        Quantity = item.Quantity,
                        UnitPrice = item.Variant.Price,
                        IsDeleted = false
                    });
                }

                order.Payment = new Payment
                {
                    Amount = total,
                    PaymentMethod = PaymentMethod.COD,
                    Status = PaymentStatus.Pending,
                    TransactionId = $"COD-{DemoOrderCode}",
                    IsDeleted = false
                };

                order.Shipment = new Shipment
                {
                    Carrier = "GHN",
                    TrackingNumber = $"TRK-{DemoOrderCode}",
                    Status = ShipmentStatus.Preparing,
                    EstimatedDeliveryDate = DateTime.UtcNow.AddDays(3),
                    IsDeleted = false
                };

                await context.Orders.AddAsync(order, cancellationToken);
                await context.SaveChangesAsync(cancellationToken);
                return;
            }

            var hasOrderChanged = false;

            if (order.IsDeleted)
            {
                order.IsDeleted = false;
                order.DeletedAt = null;
                hasOrderChanged = true;
            }

            order.SubTotal = subTotal;
            order.ShippingFee = shippingFee;
            order.TotalAmount = total;
            order.PhoneNumber = shippingAddress.PhoneNumber;
            order.Note = "Đơn hàng demo seed tự động";
            order.ExpectedDeliveryDate = DateTime.UtcNow.AddDays(3);

            order.ShippingAddress = new OrderAddressSnapshot
            {
                RecipientName = shippingAddress.FullName,
                RecipientPhone = shippingAddress.PhoneNumber,
                StreetAddress = shippingAddress.StreetLine,
                Ward = shippingAddress.Ward,
                District = shippingAddress.District,
                City = shippingAddress.City
            };

            hasOrderChanged = true;

            if (order.OrderItems.Count == 0)
            {
                foreach (var item in orderItemSeeds)
                {
                    order.OrderItems.Add(new OrderItem
                    {
                        OrderId = order.Id,
                        ProductId = item.Variant.ProductId,
                        VariantId = item.Variant.Id,
                        ProductName = item.Variant.Product?.Name ?? string.Empty,
                        Color = item.Variant.Color,
                        Size = item.Variant.Size,
                        ThumbnailUrl = item.Variant.ThumbnailUrl,
                        Quantity = item.Quantity,
                        UnitPrice = item.Variant.Price,
                        IsDeleted = false
                    });
                }
                hasOrderChanged = true;
            }

            if (order.Payment is null)
            {
                order.Payment = new Payment
                {
                    OrderId = order.Id,
                    Amount = total,
                    PaymentMethod = PaymentMethod.COD,
                    Status = PaymentStatus.Pending,
                    TransactionId = $"COD-{DemoOrderCode}",
                    IsDeleted = false
                };
                hasOrderChanged = true;
            }
            else
            {
                order.Payment.Amount = total;
                order.Payment.PaymentMethod = PaymentMethod.COD;
                order.Payment.Status = PaymentStatus.Pending;
                if (order.Payment.IsDeleted)
                {
                    order.Payment.IsDeleted = false;
                    order.Payment.DeletedAt = null;
                }

                hasOrderChanged = true;
            }

            if (order.Shipment is null)
            {
                order.Shipment = new Shipment
                {
                    OrderId = order.Id,
                    Carrier = "GHN",
                    TrackingNumber = $"TRK-{DemoOrderCode}",
                    Status = ShipmentStatus.Preparing,
                    EstimatedDeliveryDate = DateTime.UtcNow.AddDays(3),
                    IsDeleted = false
                };
                hasOrderChanged = true;
            }
            else
            {
                order.Shipment.Carrier = "GHN";
                order.Shipment.TrackingNumber = $"TRK-{DemoOrderCode}";
                order.Shipment.Status = ShipmentStatus.Preparing;
                order.Shipment.EstimatedDeliveryDate = DateTime.UtcNow.AddDays(3);
                if (order.Shipment.IsDeleted)
                {
                    order.Shipment.IsDeleted = false;
                    order.Shipment.DeletedAt = null;
                }

                hasOrderChanged = true;
            }

            if (hasOrderChanged)
            {
                context.Orders.Update(order);
                await context.SaveChangesAsync(cancellationToken);
            }
        }
    }
}
