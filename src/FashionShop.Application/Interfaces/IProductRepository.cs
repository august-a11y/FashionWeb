using FashionShop.Domain.Entities;

namespace FashionShop.Application.Interfaces
{
    public interface IProductRepository : IRepository<Product, Guid>
    {

    }
}
