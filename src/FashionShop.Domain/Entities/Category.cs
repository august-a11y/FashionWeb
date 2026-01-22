using FashionShop.Domain.Common;

namespace FashionShop.Domain.Entities
{
    public class Category : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public ICollection<Product> Products { get; set; } = new List<Product>();
        public Category(string name, string description)
        {
            if(string.IsNullOrEmpty(name))
            {
                throw new ArgumentException("Category name cannot be null or empty.", nameof(name));
            };
            Name = name;
            Description = description;
        }
        public void UpdateDetails(string Name, string Description)
        {
            this.Name = !string.IsNullOrEmpty(Name) ? Name : this.Name;
            this.Description = !string.IsNullOrEmpty(Description) ? Description : this.Description;
        }
    }
}
