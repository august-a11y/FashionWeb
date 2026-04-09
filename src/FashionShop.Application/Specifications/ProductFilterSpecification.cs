using Ardalis.Specification;
using FashionShop.Domain.Entities;

namespace FashionShop.Application.Specifications
{
    public sealed class ProductFilterSpecification : Specification<Product>
    {
        public ProductFilterSpecification(
            string? name = null,
            Guid? categoryId = null,
            IReadOnlyCollection<Guid>? categoryIds = null,
            decimal? minPrice = null,
            decimal? maxPrice = null,
            string? sortBy = null,
            bool sortDescending = true,
            int pageIndex = 1,
            int pageSize = 20)
        {
            // 1. TỐI ƯU BỘ LỌC (Chỉ sinh SQL khi có dữ liệu thật)
            var normalizedName = name?.Trim();
            if (!string.IsNullOrWhiteSpace(normalizedName))
            {
                Query.Where(p => p.Name.Contains(normalizedName));
            }

            if (categoryId.HasValue)
            {
                Query.Where(p => p.CategoryId == categoryId.Value);
            }

            if (categoryIds != null && categoryIds.Count > 0)
            {
                Query.Where(p => categoryIds.Contains(p.CategoryId));
            }

            if (minPrice.HasValue)
            {
                Query.Where(p => p.BasePrice >= minPrice.Value);
            }

            if (maxPrice.HasValue)
            {
                Query.Where(p => p.BasePrice <= maxPrice.Value);
            }

            // 2. INCLUDE BẢNG LIÊN QUAN
            Query.Include(p => p.Category);

            // 3. SẮP XẾP
            switch (sortBy?.Trim().ToLowerInvariant())
            {
                case "name":
                    if (sortDescending) Query.OrderByDescending(p => p.Name);
                    else Query.OrderBy(p => p.Name);
                    break;
                case "price":
                    if (sortDescending) Query.OrderByDescending(p => p.BasePrice);
                    else Query.OrderBy(p => p.BasePrice);
                    break;
                default: // Mặc định sắp xếp theo ngày tạo mới nhất
                    if (sortDescending) Query.OrderByDescending(p => p.CreatedAt);
                    else Query.OrderBy(p => p.CreatedAt);
                    break;
            }

            // 4. PHÂN TRANG
            int validPageIndex = pageIndex < 1 ? 1 : pageIndex;
            int validPageSize = pageSize < 1 ? 20 : pageSize;

            Query.Skip((validPageIndex - 1) * validPageSize)
                 .Take(validPageSize);
        }
    }
}