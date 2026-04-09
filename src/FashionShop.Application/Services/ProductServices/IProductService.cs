using FashionShop.Application.Common.DTOs;
using FashionShop.Application.Services.ProductServices.DTO;
using FluentResults;

namespace FashionShop.Application.Services.ProductServices
{
    public interface IProductService
    {
        Task<Result<ProductResponseDTO>> GetProductByIdAsync(Guid id, CancellationToken cancellationToken);
        Task<Result<PageResponse<ProductResponseDTO>>> GetAllProductAsync(int pageIndex, int pageSize, CancellationToken cancellationToken);
        Task<Result<ProductResponseDTO>> CreateProductAsync(CreateProductDTO product, CancellationToken cancellationToken);
        Task<Result> DeleteProductAsync(Guid id, CancellationToken cancellationToken);
        Task<Result<ProductResponseDTO>> UpdateProductAsync(Guid id, UpdateDetailsProductDTO product, CancellationToken cancellationToken);

        Task<Result<PageResponse<ProductResponseDTO>>> GetProductsByCategoryAsync(
            int pageIndex,
            int pageSize,
            Guid categoryId,
            CancellationToken cancellationToken);
        Task<Result<PageResponse<ProductResponseDTO>>> GetProductsByListCategoryIdAsync(
            List<Guid> categoryIds,
            int pageIndex,
            int pageSize,
            CancellationToken cancellationToken);
    }
}
