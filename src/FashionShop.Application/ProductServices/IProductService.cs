using FashionShop.Application.ProductServices.DTO;
using FluentResults;

namespace FashionShop.Application.ProductServices
{
    public interface IProductService
    {
        Task<Result<ProductResponseDTO>> GetProductByIdAsync(Guid id, CancellationToken cancellationToken);
        Task<Result<IEnumerable<ProductResponseDTO>>> GetAllProductAsync(CancellationToken cancellationToken);
        Task<Result<ProductResponseDTO>> CreateProductAsync(CreateProductDTO product, CancellationToken cancellationToken);
        Task<Result> DeleteProductAsync(Guid id, CancellationToken cancellationToken);
        Task<Result<ProductResponseDTO>> UpdateProductAsync(Guid id, UpdateDetailsProductDTO product, CancellationToken cancellationToken);
    }
}
