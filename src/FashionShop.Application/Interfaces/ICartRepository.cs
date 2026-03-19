using FashionShop.Domain.Entities;

namespace FashionShop.Application.Interfaces
{
    public interface ICartRepository : IRepository<Cart, Guid>
    {
        public Task<Cart?> GetCartWithItemsByUserIdAsync(Guid? userId , CancellationToken cancellationToken);
        
    }
}
