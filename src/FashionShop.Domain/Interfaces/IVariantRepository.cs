using FashionShop.Domain.Entities;
namespace FashionShop.Domain.Interfaces
{
    public interface IVariantRepository : IRepository<Variant, Guid>
    {
        Task<bool> DecreaseStockAsync(Guid productId, Guid variantId, int quantity, CancellationToken cancellationToken);
        Task<List<Variant>> GetByIdWithProductAsync(List<Guid> variantIds, CancellationToken cancellationToken);
        Task<List<Variant>> GetListByIdsWithProductAsync(List<Guid> variantIds, CancellationToken cancellationToken);
        Task<List<Variant>> GetListByProductIdAsync(Guid productId, CancellationToken cancellation);
    }
}
