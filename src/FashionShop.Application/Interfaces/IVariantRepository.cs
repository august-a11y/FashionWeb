using FashionShop.Domain.Entities;
namespace FashionShop.Application.Interfaces
{
    public interface IVariantRepository : IRepository<Variant>
    {
        Task<bool> DecreaseStockAsync(Guid productId, Guid variantId, int quantity, CancellationToken cancellationToken);
        Task<Variant> GetByIdWithProductAsync(Guid variantId, CancellationToken cancellationToken);
        Task<List<Variant>> GetListByIdsWithProductAsync(List<Guid> variantIds, CancellationToken cancellationToken);
        Task<List<Variant>> GetListByProductIdAsync(Guid productId, CancellationToken cancellation);

    }
}
