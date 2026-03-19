using FashionShop.Domain.Common;

namespace FashionShop.Domain.Entities
{
    public class Product : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal? BasePrice { get; set; }

        public string ThumbnailUrl { get; set; } = string.Empty;
        public Guid CategoryId { get; set; }
        public virtual Category Category { get; set; }
        public ICollection<Variant> Variants { get; set; } = new List<Variant>();
        
        //public Product(string name, string description, string slug, decimal price, string thumbnailUrl, Guid categoryId)
        //{
        //    Name = name;
        //    BasePrice = price;
        //    Description = description;
        //    ThumbnailUrl = thumbnailUrl;
        //    CategoryId = categoryId;
        //}

        public void UpdateDetails(string? name, string? desc, string? img)
        {
            Name = !string.IsNullOrEmpty(name) ? name : Name;
            Description = !string.IsNullOrEmpty(desc) ? desc : Description;
            ThumbnailUrl = !string.IsNullOrEmpty(img) ? img : ThumbnailUrl;
        }

        public void ChangePrice(decimal? newPrice)
        {
            if (newPrice < 0) throw new ArgumentException("Giá không hợp lệ");
            if (newPrice.HasValue)
            {
                BasePrice = newPrice;
            }
        }


    }

    public class Variant : BaseEntity
    {
        public Guid ProductId { get; set; }
        public virtual Product Product { get; set; } = null!;
        public string Size { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
        public string ThumbnailUrl { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
        public void UpdateDetails(string size, decimal? price, string collor, int? stock)
        {

            Size = !string.IsNullOrEmpty(size) ? size : Size;
            Price = price ?? Price;
            StockQuantity = stock ?? StockQuantity;
            Color = !string.IsNullOrEmpty(collor) ? collor : Color;
        }
    }
}
