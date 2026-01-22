using FashionShop.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FashionShop.Application.Extensions
{
    public static class ProductQueryExtensions
    {
        public static IQueryable<Product> FilterByName(this IQueryable<Product> query, string name)
        {
            if(string.IsNullOrEmpty(name)) return query;
            return query.Where(p => p.Name == name);
        }
        public static IQueryable<Product> FilterByCategory(this IQueryable<Product> query, Guid? categoryId)
        {
            if (!categoryId.HasValue)
                return query;

            return query.Where(p => p.CategoryId == categoryId.Value);
        }

        // 2. Lọc theo Giá (Khoảng giá)
        public static IQueryable<Product> FilterByPriceRange(this IQueryable<Product> query, decimal? minPrice, decimal? maxPrice)
        {
            if (minPrice.HasValue)
                query = query.Where(p => p.BasePrice >= minPrice.Value);

            if (maxPrice.HasValue)
                query = query.Where(p => p.BasePrice <= maxPrice.Value);

            return query;
        }

        // 3. Sắp xếp động (Quan trọng)
        public static IQueryable<Product> ApplySorting(this IQueryable<Product> query, string sortColumn, string sortOrder)
        {
            // Mặc định sắp xếp theo ngày tạo mới nhất nếu không chọn gì
            if (string.IsNullOrWhiteSpace(sortColumn))
            {
                return query.OrderByDescending(p => p.CreatedAt);
            }

            // Chuẩn hóa input (đưa về chữ thường để so sánh)
            var column = sortColumn.Trim().ToLower();
            var isAscending = sortOrder?.Trim().ToLower() == "asc";

            // Dùng switch expression để chọn cột sắp xếp
            return column switch
            {
                "price" => isAscending
                            ? query.OrderBy(p => p.BasePrice)
                            : query.OrderByDescending(p => p.BasePrice),

                "name" => isAscending
                            ? query.OrderBy(p => p.Name)
                            : query.OrderByDescending(p => p.Name),

                "date" => isAscending
                            ? query.OrderBy(p => p.CreatedAt)
                            : query.OrderByDescending(p => p.CreatedAt),

                // Mặc định (Default)
                _ => query.OrderByDescending(p => p.CreatedAt)
            };
        }
    }
}
