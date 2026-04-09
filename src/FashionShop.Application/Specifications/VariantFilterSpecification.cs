using Ardalis.Specification;
using FashionShop.Domain.Entities;

namespace FashionShop.Application.Specifications
{
    public sealed class VariantFilterSpecification : Specification<Variant>
    {
        public VariantFilterSpecification(
            Guid? variantId = null,
            Guid? productId = null,
            string? size = null,
            string? color = null,
            decimal? minPrice = null,
            decimal? maxPrice = null,
            bool? inStockOnly = null,
            string? sortBy = null,
            bool sortDescending = true)
        {

            if (variantId.HasValue) 
            {
                Query.Where(v => v.Id == variantId.Value);
            }

            if (productId.HasValue)
            {
                Query.Where(v => v.ProductId == productId.Value);
            }

            if (!string.IsNullOrWhiteSpace(size))
            {
                Query.Where(v => v.Size.ToLower() == size.Trim().ToLower());
            }

            if (!string.IsNullOrWhiteSpace(color))
            {
                Query.Where(v => v.Color == color.Trim());
            }

            if (minPrice.HasValue)
            {
                Query.Where(v => v.Price >= minPrice.Value);
            }

            if (maxPrice.HasValue)
            {
                Query.Where(v => v.Price <= maxPrice.Value);
            }

            if (inStockOnly == true)
            {
                Query.Where(v => v.StockQuantity > 0);
            }
            Query.Include(v => v.Product);
            switch (sortBy?.Trim().ToLowerInvariant())
            {
                case "price":
                    if (sortDescending) Query.OrderByDescending(v => v.Price);
                    else Query.OrderBy(v => v.Price);
                    break;

                case "stock":
                case "stockquantity":
                    if (sortDescending) Query.OrderByDescending(v => v.StockQuantity);
                    else Query.OrderBy(v => v.StockQuantity);
                    break;

                case "date":
                case "createdat":
                default:
                    Query.OrderByDescending(v => v.CreatedAt);
                    break;
            }
        }
    }
}