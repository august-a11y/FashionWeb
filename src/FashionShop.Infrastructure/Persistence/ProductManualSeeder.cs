using FashionShop.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FashionShop.Infrastructure.Persistence
{
    public static class ProductManualSeeder
    {
        private const int TargetSeedProducts = 200;

        public static async Task SeedBulkProductsAsync(
            ApplicationDbContext context,
            Guid aoThunCategoryId,
            Guid aoPoloCategoryId,
            Guid aoSoMiCategoryId,
            Guid aoKhoacCategoryId,
            Guid quanJeansCategoryId,
            Guid quanTayCategoryId,
            CancellationToken cancellationToken)
        {
            var existingCount = await context.Products
                .IgnoreQueryFilters()
                .CountAsync(cancellationToken);

            if (existingCount >= TargetSeedProducts)
            {
                return;
            }

            var categoryIds = new[]
            {
                aoThunCategoryId,
                aoPoloCategoryId,
                aoSoMiCategoryId,
                aoKhoacCategoryId,
                quanJeansCategoryId,
                quanTayCategoryId
            };

            var existingCounts = await context.Products
                .IgnoreQueryFilters()
                .Where(p => categoryIds.Contains(p.CategoryId))
                .GroupBy(p => p.CategoryId)
                .Select(g => new { CategoryId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.CategoryId, x => x.Count, cancellationToken);

            const int targetPerCategory = 30;
            var neededPerCategory = categoryIds.ToDictionary(
                id => id,
                id => Math.Max(0, targetPerCategory - existingCounts.GetValueOrDefault(id)));

            var categoryFolderMap = new Dictionary<Guid, string>
            {
                [aoThunCategoryId] = "ao_thun",
                [aoPoloCategoryId] = "ao_polo",
                [aoSoMiCategoryId] = "ao_so_mi",
                [aoKhoacCategoryId] = "ao_khoac",
                [quanJeansCategoryId] = "quan_jeans",
                [quanTayCategoryId] = "quan_tay"
            };

            var imgRootPath = ResolveImgRootPath();

            var startIndex = existingCount + 1;
            var remaining = TargetSeedProducts - existingCount;

            var templatesByCategory = new Dictionary<Guid, (string Name, string Description, decimal Price, Guid CategoryId)[]>
            {
                [aoThunCategoryId] = new[]
                {
                    ("Áo thun basic", "Áo thun cotton thoáng mát, form regular.", 199000m, aoThunCategoryId),
                    ("Áo thun oversize", "Form rộng trẻ trung, chất cotton dày dặn.", 239000m, aoThunCategoryId),
                    ("Áo thun in graphic", "Họa tiết in nổi bật, phù hợp streetwear.", 259000m, aoThunCategoryId),
                    ("Áo thun cổ tròn premium", "Vải mềm mịn, co giãn tốt, mặc thoải mái cả ngày.", 289000m, aoThunCategoryId),
                    ("Áo thun thể thao", "Chất liệu thấm hút mồ hôi, nhanh khô.", 269000m, aoThunCategoryId)
                },
                [aoPoloCategoryId] = new[]
                {
                    ("Áo polo premium", "Áo polo chất liệu pique co giãn nhẹ.", 329000m, aoPoloCategoryId),
                    ("Áo polo basic", "Thiết kế tối giản, dễ phối quần jeans/chinos.", 299000m, aoPoloCategoryId),
                    ("Áo polo phối viền", "Cổ và tay phối màu nổi bật, phong cách năng động.", 349000m, aoPoloCategoryId),
                    ("Áo polo slimfit", "Dáng ôm vừa vặn, tôn dáng.", 359000m, aoPoloCategoryId),
                    ("Áo polo cao cấp", "Chất vải dày vừa, bề mặt mịn, đứng form.", 389000m, aoPoloCategoryId)
                },
                [aoSoMiCategoryId] = new[]
                {
                    ("Áo sơ mi công sở", "Sơ mi dài tay phù hợp đi làm.", 359000m, aoSoMiCategoryId),
                    ("Áo sơ mi ngắn tay", "Thoáng mát, phù hợp thời tiết nóng.", 319000m, aoSoMiCategoryId),
                    ("Áo sơ mi trắng basic", "Kiểu dáng cổ điển, dễ phối đồ.", 339000m, aoSoMiCategoryId),
                    ("Áo sơ mi kẻ sọc", "Họa tiết sọc thanh lịch, phù hợp công sở.", 379000m, aoSoMiCategoryId),
                    ("Áo sơ mi linen", "Chất linen nhẹ, thoáng, phong cách tối giản.", 399000m, aoSoMiCategoryId)
                },
                [aoKhoacCategoryId] = new[]
                {
                    ("Áo khoác hoodie nỉ", "Hoodie nỉ mềm, giữ ấm tốt.", 449000m, aoKhoacCategoryId),
                    ("Áo khoác gió", "Khoác gió chống nhẹ nước, mặc hằng ngày.", 499000m, aoKhoacCategoryId),
                    ("Áo khoác bomber", "Thiết kế năng động, bo tay và gấu áo.", 529000m, aoKhoacCategoryId),
                    ("Áo khoác denim", "Chất denim bền, phong cách casual.", 559000m, aoKhoacCategoryId),
                    ("Áo khoác dù 2 lớp", "Nhẹ, cản gió tốt, phù hợp đi ngoài trời.", 579000m, aoKhoacCategoryId)
                },
                [quanJeansCategoryId] = new[]
                {
                    ("Quần jeans slimfit", "Jeans co giãn nhẹ, tôn dáng.", 399000m, quanJeansCategoryId),
                    ("Quần jeans basic", "Jeans form regular, dễ phối đồ.", 369000m, quanJeansCategoryId),
                    ("Quần jeans rách nhẹ", "Thiết kế rách nhẹ cá tính, trẻ trung.", 429000m, quanJeansCategoryId),
                    ("Quần jeans ống suông", "Form rộng thoải mái, xu hướng hiện đại.", 439000m, quanJeansCategoryId),
                    ("Quần jeans xanh đậm", "Màu wash đậm, phù hợp nhiều hoàn cảnh.", 419000m, quanJeansCategoryId)
                },
                [quanTayCategoryId] = new[]
                {
                    ("Quần tây công sở", "Quần tây đứng form, lịch sự.", 429000m, quanTayCategoryId),
                    ("Quần tây slim", "Quần tây dáng slim, hiện đại.", 439000m, quanTayCategoryId),
                    ("Quần tây premium", "Chất vải mịn, phù hợp đi làm/sự kiện.", 499000m, quanTayCategoryId),
                    ("Quần tây ống đứng", "Dáng cổ điển, dễ kết hợp áo sơ mi.", 459000m, quanTayCategoryId),
                    ("Quần tây co giãn", "Vải có độ co giãn, mặc thoải mái cả ngày.", 479000m, quanTayCategoryId)
                }
            };

            var categoryPlan = new List<Guid>(remaining);

            foreach (var categoryId in categoryIds)
            {
                var needed = neededPerCategory[categoryId];
                for (var i = 0; i < needed && categoryPlan.Count < remaining; i++)
                {
                    categoryPlan.Add(categoryId);
                }
            }

            for (var i = 0; categoryPlan.Count < remaining; i++)
            {
                categoryPlan.Add(categoryIds[i % categoryIds.Length]);
            }

            var templateCursor = categoryIds.ToDictionary(id => id, _ => 0);
            var products = new List<Product>(remaining);

            for (var i = 0; i < remaining; i++)
            {
                var globalIndex = startIndex + i;
                var categoryId = categoryPlan[i];

                var templates = templatesByCategory[categoryId];
                var template = templates[templateCursor[categoryId] % templates.Length];
                templateCursor[categoryId]++;

                var thumbnailUrl = GetRandomThumbnailUrlForCategory(categoryId, categoryFolderMap, imgRootPath);

                products.Add(new Product
                {
                    Name = $"{template.Name} {globalIndex:000}",
                    Description = template.Description,
                    BasePrice = template.Price,
                    CategoryId = template.CategoryId,
                    ThumbnailUrl = thumbnailUrl,
                    IsDeleted = false
                });
            }

            await context.Products.AddRangeAsync(products, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);
        }

        private static string? ResolveImgRootPath()
        {
            var root = Directory.GetCurrentDirectory();
            var candidates = new[]
            {
                // chạy từ workspace root
                Path.Combine(root, "src", "FashionShop.API", "img"),

                // chạy từ API project root
                Path.Combine(root, "img"),

                // fallback khi chạy từ bin
                Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "img"))
            };

            return candidates.FirstOrDefault(Directory.Exists);
        }

        private static string GetRandomThumbnailUrlForCategory(
            Guid categoryId,
            IReadOnlyDictionary<Guid, string> categoryFolderMap,
            string? imgRootPath)
        {
            if (string.IsNullOrWhiteSpace(imgRootPath))
            {
                return "/img/default-product.jpg";
            }

            if (!categoryFolderMap.TryGetValue(categoryId, out var folderName))
            {
                return "/img/default-product.jpg";
            }

            var randomNumber = Random.Shared.Next(1, 11);
            var fileBaseName = randomNumber.ToString();
            var validExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };

            foreach (var extension in validExtensions)
            {
                var physicalPath = Path.Combine(imgRootPath, folderName, $"{fileBaseName}{extension}");
                if (File.Exists(physicalPath))
                {
                    return $"/img/{folderName}/{fileBaseName}{extension}";
                }
            }

            return "/img/default-product.jpg";
        }
    }
}
