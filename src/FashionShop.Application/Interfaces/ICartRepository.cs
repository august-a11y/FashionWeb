using FashionShop.Domain.Entities;

namespace FashionShop.Application.Interfaces
{
    public interface ICartRepository : IRepository<Cart>
    {
        public Task<Cart?> GetCartWithItemsByUserIdAsync(Guid? userId , CancellationToken cancellationToken);
        
    }
}
