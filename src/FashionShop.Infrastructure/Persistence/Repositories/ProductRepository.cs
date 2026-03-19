using FashionShop.Application.Interfaces;
using FashionShop.Domain.Entities;

namespace FashionShop.Infrastructure.Persistence.Repositories
{
    public class ProductRepository : Repository<Product, Guid>, IProductRepository
    {
        public ProductRepository(ApplicationDbContext dbContext) : base(dbContext)
        {
        }
    }
}
