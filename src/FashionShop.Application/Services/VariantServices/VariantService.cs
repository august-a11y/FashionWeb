using FashionShop.Application.Interfaces;
using FashionShop.Application.Services.VariantServices.DTO;
using FashionShop.Application.Specifications;
using FashionShop.Domain.Entities;
using FluentResults;

namespace FashionShop.Application.Services.VariantServices
{
    public class VariantService : IVariantService
    {
        private readonly IRepository<Variant> _variantRepository;
        private readonly IRepository<Product> _productRepository;
        private readonly IUnitOfWork _unitOfWork;

        public VariantService(
            IRepository<Variant> variantRepository,
            IRepository<Product> productRepository,
            IUnitOfWork unitOfWork)
        {
            _variantRepository = variantRepository;
            _productRepository = productRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<List<VariantDTO>>> GetVariantsByProductIdAsync(Guid productId, CancellationToken cancellationToken)
        {
            if (productId == Guid.Empty)
                return Result.Fail<List<VariantDTO>>("Invalid product id.");

            var specification = new VariantFilterSpecification(productId: productId, inStockOnly: null);
            var variants = await _variantRepository.ListAsync(specification, cancellationToken) ?? new List<Variant>();

            var result = variants
                .Select(MapToDto)
                .ToList();

            return Result.Ok(result);
        }

        public async Task<Result<VariantDTO>> GetVariantByIdAsync(Guid variantId, CancellationToken cancellationToken)
        {
            if (variantId == Guid.Empty)
                return Result.Fail<VariantDTO>("Invalid variant id.");
            var specification = new VariantFilterSpecification(variantId: variantId);
            var root = await _variantRepository.ListAsync(specification, cancellationToken);
            var variant = root?.FirstOrDefault();
            if (variant == null)
                return Result.Fail<VariantDTO>("Variant not found.");

            return Result.Ok(MapToDto(variant));
        }

        public async Task<Result<VariantDTO>> CreateVariantAsync(CreateVariantDTO createVariantDto, CancellationToken cancellationToken)
        {
            if (createVariantDto.ProductId == Guid.Empty)
                return Result.Fail<VariantDTO>("Invalid product id.");

            if (createVariantDto.Price < 0)
                return Result.Fail<VariantDTO>("Price must be greater than or equal to 0.");

            if (createVariantDto.StockQuantity < 0)
                return Result.Fail<VariantDTO>("Stock quantity must be greater than or equal to 0.");

            var product = await _productRepository.GetByIdAsync(createVariantDto.ProductId, cancellationToken);
            if (product == null)
                return Result.Fail<VariantDTO>("Product not found.");

            var variant = new Variant
            {
                ProductId = createVariantDto.ProductId,
                Size = createVariantDto.Size,
                Color = createVariantDto.Color,
                ThumbnailUrl = createVariantDto.ThumbnailUrl,
                Price = createVariantDto.Price,
                StockQuantity = createVariantDto.StockQuantity
            };

            await _variantRepository.AddAsync(variant, cancellationToken);
            await _unitOfWork.CommitAsync(cancellationToken);

            return Result.Ok(MapToDto(variant));
        }

        public async Task<Result<VariantDTO>> UpdateVariantAsync(Guid variantId, UpdateVariantDTO updateVariantDto, CancellationToken cancellationToken)
        {
            if (variantId == Guid.Empty)
                return Result.Fail<VariantDTO>("Invalid variant id.");

            if (updateVariantDto.Price.HasValue && updateVariantDto.Price.Value < 0)
                return Result.Fail<VariantDTO>("Price must be greater than or equal to 0.");

            if (updateVariantDto.StockQuantity.HasValue && updateVariantDto.StockQuantity.Value < 0)
                return Result.Fail<VariantDTO>("Stock quantity must be greater than or equal to 0.");

            var variant = await _variantRepository.GetByIdAsync(variantId, cancellationToken);
            if (variant == null)
                return Result.Fail<VariantDTO>("Variant not found.");

            variant.UpdateDetails(
                updateVariantDto.Size ?? string.Empty,
                updateVariantDto.Price,
                updateVariantDto.Color ?? string.Empty,
                updateVariantDto.StockQuantity);

            if (!string.IsNullOrWhiteSpace(updateVariantDto.ThumbnailUrl))
                variant.ThumbnailUrl = updateVariantDto.ThumbnailUrl;

            await _unitOfWork.CommitAsync(cancellationToken);

            return Result.Ok(MapToDto(variant));
        }

        public async Task<Result<bool>> DeleteVariantAsync(Guid variantId, CancellationToken cancellationToken)
        {
            if (variantId == Guid.Empty)
                return Result.Fail<bool>("Invalid variant id.");

            var variant = await _variantRepository.GetByIdAsync(variantId, cancellationToken);
            if (variant == null)
                return Result.Fail<bool>("Variant not found.");

            await _variantRepository.DeleteAsync(variant, cancellationToken);
            await _unitOfWork.CommitAsync(cancellationToken);

            return Result.Ok(true);
        }

        private static VariantDTO MapToDto(Variant variant)
        {
            return new VariantDTO
            {
                Id = variant.Id,
                ProductName = variant.Product?.Name ?? string.Empty,
                Size = variant.Size,
                Color = variant.Color,
                ThumbnailUrl = variant.ThumbnailUrl,
                Price = variant.Price,
                StockQuantity = variant.StockQuantity
            };
        }
    }
}