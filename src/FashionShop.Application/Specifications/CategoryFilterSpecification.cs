using Ardalis.Specification;
using FashionShop.Domain.Entities;

namespace FashionShop.Application.Specifications
{
    public sealed class CategoryFilterSpecification : Specification<Category>
    {
        public CategoryFilterSpecification(
            Guid? categoryId = null,
            string? name = null,
            Guid? parentCategoryId = null,
            bool? rootOnly = null,
            string? sortBy = null,
            bool sortDescending = false)
        {
            if (categoryId.HasValue)
            {
                Query.Where(c => c.Id == categoryId.Value);
            }

            if (!string.IsNullOrWhiteSpace(name))
            {
                Query.Where(c => c.Name.Contains(name.Trim()));
            }

            if (parentCategoryId.HasValue)
            {
                Query.Where(c => c.ParentCategoryId == parentCategoryId.Value);
            }

            if (rootOnly == true)
            {
                Query.Where(c => c.ParentCategoryId == null);
            }

            Query.Include(c => c.Children);
            switch (sortBy?.Trim().ToLowerInvariant())
            {
                case "createdat":
                case "date":
                    if (sortDescending)
                        Query.OrderByDescending(c => c.CreatedAt);
                    else
                        Query.OrderBy(c => c.CreatedAt);
                    break;

                case "name":
                default:
                    if (sortDescending)
                        Query.OrderByDescending(c => c.Name);
                    else
                        Query.OrderBy(c => c.Name);
                    break;
            }
        }
    }
}