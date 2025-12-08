using FashionShop.Domain.Entities;
using FashionShop.Domain.Interfaces;
using FashionShop.Infrastructure.Persistence;

namespace FashionShop.Infrastructure.Repositories
{
    public class ProductRepository : IProductRepository
    {
        private readonly ApplicationDbContext _context;

        public ProductRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<int> AddAsync(Product product)
        {
            await _context.Products.AddAsync(product);
            await _context.SaveChangesAsync();
            return Task.FromResult(product.Id).Result;
        }

        public async Task DeleteAsync(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product != null)
            {
                product.IsDeleted = true;
                _context.Products.Remove(product);
                await _context.SaveChangesAsync();
            }
            else
            {
                throw new KeyNotFoundException($"Product with ID {id} not found.");
            }

        }

        public Task<List<Product>> GetAllAsync()
        {
            var products = _context.Products.Where(p => !p.IsDeleted).ToList();
            if (products.Count == 0)
            {
                throw new KeyNotFoundException("No products found.");
            }
            return Task.FromResult(products);
        }

        public Task<Product?> GetByIdAsync(int id)
        {
            var product = _context.Products.FirstOrDefault(p => p.Id == id && !p.IsDeleted);
            return Task.FromResult(product);
        }

        public Task UpdateAsync(Product product)
        {
            var productToUpdate = _context.Products.FirstOrDefault(p => p.Id == product.Id && !p.IsDeleted);
            if (productToUpdate != null)
            {
                productToUpdate.Name = product.Name;
                productToUpdate.Description = product.Description;
                productToUpdate.Price = product.Price;
                productToUpdate.ImageUrl = product.ImageUrl;
                productToUpdate.Size = product.Size;
                productToUpdate.Color = product.Color;
                productToUpdate.StockQuantity = product.StockQuantity;
                productToUpdate.CategoryId = product.CategoryId;
                _context.Products.Update(productToUpdate);
                return _context.SaveChangesAsync();
            }
            else
            {
                throw new KeyNotFoundException($"Product with ID {product.Id} not found.");
            }
        }
    }
}
