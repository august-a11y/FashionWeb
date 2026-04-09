using FashionShop.Application.Services.VariantServices.DTO;
using FluentResults;

namespace FashionShop.Application.Services.VariantServices
{
    public interface IVariantService
    {
        Task<Result<List<VariantDTO>>> GetVariantsByProductIdAsync(Guid productId, CancellationToken cancellationToken);
        Task<Result<VariantDTO>> GetVariantByIdAsync(Guid variantId, CancellationToken cancellationToken);
        Task<Result<VariantDTO>> CreateVariantAsync(CreateVariantDTO createVariantDto, CancellationToken cancellationToken);
        Task<Result<VariantDTO>> UpdateVariantAsync(Guid variantId, UpdateVariantDTO updateVariantDto, CancellationToken cancellationToken);
        Task<Result<bool>> DeleteVariantAsync(Guid variantId, CancellationToken cancellationToken);
    }
}