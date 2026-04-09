using FashionShop.Domain.Common;

namespace FashionShop.Domain.Entities
{
    public class Category : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        public Guid? ParentCategoryId { get; set; }
        public virtual Category? ParentCategory { get; set; }
        public virtual ICollection<Category> Children { get; set; } = new List<Category>();

        public virtual ICollection<Product> Products { get; set; } = new List<Product>();

        public Category(string name, string description, Guid? parentCategoryId = null)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException("Category name cannot be null or empty.", nameof(name));
            }

            Name = name;
            Description = description;
            ParentCategoryId = parentCategoryId;
        }

        public void UpdateDetails(string? name, string? description)
        {
            Name = !string.IsNullOrEmpty(name) ? name : Name;
            Description = !string.IsNullOrEmpty(description) ? description : Description;
        }
    }
}
