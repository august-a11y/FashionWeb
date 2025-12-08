using FashionShop.Domain.Entities;
using FashionShop.Domain.Interfaces;
using FashionShop.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;


namespace FashionShop.Infrastructure.Repositories
{
    public class CategoryRepository : ICategoryRepository
    {
        private readonly ApplicationDbContext _dbContext;

        public CategoryRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<int> CreateCategory(Category category)
        {
            await _dbContext.Set<Category>().AddAsync(category);
            return category.Id;
        }

        public async Task DeleteCategory(int id)
        {
            await _dbContext.Set<Category>().Where(c => c.Id == id).ExecuteDeleteAsync();
        }

        public async Task<Category> GetById(int id)
        {
            var Category = await _dbContext.Set<Category>().FindAsync(id);
            if(Category == null)
            {
                throw new KeyNotFoundException($"Category with ID {id} not found.");
            }
            return Task.FromResult(Category).Result;
        }

        public async Task<List<Category>> GetCategories()
        {
            var categories = await  _dbContext.Set<Category>().Where(p => !p.IsDeleted).ToListAsync();
            if (categories.Count == 0)
            {
                throw new KeyNotFoundException("No categories found.");
            }
            return Task.FromResult(categories).Result;
        }

        public Task<Category> GetCategoryByName(string name)
        {
            var category =  _dbContext.Set<Category>().FirstOrDefault(c => c.Name == name);
            if (category == null)
            {
                throw new KeyNotFoundException($"Category with name {name} not found.");
            }
            return Task.FromResult(category);
        }

        public Task UpdateCategory(Category category)
        {
            var existingCategory =  _dbContext.Set<Category>().Find(category.Id);
            if (existingCategory == null)
            {
                throw new KeyNotFoundException($"Category with ID {category.Id} not found");
            }
            existingCategory.Name = category.Name;
            existingCategory.Products = category.Products;
            return Task.CompletedTask;
        }
    }
}
