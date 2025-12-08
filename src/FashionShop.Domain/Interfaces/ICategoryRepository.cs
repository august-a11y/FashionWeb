using FashionShop.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FashionShop.Domain.Interfaces
{
    public interface ICategoryRepository
    {
        public Task<List<Category>> GetCategories();

        public Task<Category> GetById(int id);

        public Task<Category> GetCategoryByName(string name);

        public Task<int> CreateCategory(Category category);
        
        public Task UpdateCategory(Category category);
        public Task DeleteCategory(int id);
    }
}
