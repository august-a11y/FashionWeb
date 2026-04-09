using FashionShop.Application.Common.DTOs;
using FashionShop.Application.Interfaces;
using FashionShop.Application.Services.ProductServices.DTO;
using FashionShop.Application.Specifications;
using FashionShop.Domain.Entities;
using FluentResults;
using static FashionShop.Domain.Constants.Permissions;

namespace FashionShop.Application.Services.ProductServices
{
    public class ProductService : IProductService
    {
        private const string BaseUri = "https://localhost:7239";
        private readonly IRepository<Product> _productRepository;
        private readonly IUnitOfWork _unitOfWork;
        public ProductService(IRepository<Product> productRepository, IUnitOfWork unitOfWork)
        {
            _productRepository = productRepository;
            _unitOfWork = unitOfWork;
        }
        public async Task<Result<ProductResponseDTO>> CreateProductAsync(CreateProductDTO productDto, CancellationToken cancellationToken)
        {
            if (productDto.StockQuantity < 0)
            {
                return Result.Fail<ProductResponseDTO>("Stock quantity must be greater than or equal to 0.");
            }

            var product = new Product
            {
                Name = productDto.Name,
                Description = productDto.Description,
                BasePrice = productDto.BasePrice,
                ThumbnailUrl = productDto.ThumbnailUrl,
                CategoryId = productDto.CategoryId,
                Variants = new List<Variant>
                {
                    new()
                    {
                        Size = "Default",
                        Color = "Default",
                        ThumbnailUrl = productDto.ThumbnailUrl,
                        Price = productDto.BasePrice,
                        StockQuantity = productDto.StockQuantity
                    }
                }
            };
            await _productRepository.AddAsync(product, cancellationToken);
            await _unitOfWork.CommitAsync(cancellationToken);
            return Result.Ok( new ProductResponseDTO
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Price = product.BasePrice ?? 0,
                ThumbnailUrl = product.ThumbnailUrl,
                CategoryId = product.CategoryId
            });
        }

        public async Task<Result> DeleteProductAsync(Guid id, CancellationToken cancellationToken)
        {
            
            var product = await _productRepository.GetByIdAsync(id, cancellationToken);
            if (product == null)
            {
                return Result.Fail($"Product with id {id} not found.");
            }
            else
            {
                await _productRepository.DeleteAsync(product, cancellationToken);
                await _unitOfWork.CommitAsync(cancellationToken);
                return Result.Ok();
            }
           
        }
        public async Task<Result<PageResponse<ProductResponseDTO>>> GetProductsByCategoryAsync(
            int pageIndex,
            int pageSize,
            Guid categoryId,
            CancellationToken cancellationToken)
        {
            try
            {

                var filterSpecification = new ProductFilterSpecification(null, categoryId);
                var root = await _productRepository.PagedListAsync(filterSpecification, pageIndex, pageSize, cancellationToken);
                var total = root.Total;

                var itemOnPage = root
                    .Items
                    .Select(MapProductToProductResponseDTO)
                    .ToList();

                var response = new PageResponse<ProductResponseDTO>(itemOnPage, total, pageSize, pageIndex);
                return Result.Ok(response);
            }
            catch (Exception ex)
            {
                return Result.Fail($"Failed to retrieve products: {ex}");
            }
        }

        public async Task<Result<PageResponse<ProductResponseDTO>>> GetAllProductAsync(int pageIndex, int pageSize, CancellationToken cancellationToken)
        {
            try
            {
                var filterSpecification = new ProductFilterSpecification(null, null, pageIndex: pageIndex, pageSize: pageSize);
                var root = await _productRepository.PagedListAsync(filterSpecification, pageIndex, pageSize, cancellationToken);
                var products = root.Items
                    .Select(MapProductToProductResponseDTO)
                    .ToList();
                var response = new PageResponse<ProductResponseDTO>(products, root.Total, pageSize, pageIndex);
                return Result.Ok(response);
            }
            catch (Exception ex)
            {
                return Result.Fail($"Failed to retrieve products:  {ex}");
            }
                
        }

        public async Task<Result<ProductResponseDTO>> GetProductByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            var product = await _productRepository.GetByIdAsync(id, cancellationToken);
            if (product == null)
            {
                return Result.Fail($"Product with id {id} not found.");
            }
            else
            {
                return Result.Ok(MapProductToProductResponseDTO(product));
            }
        }

        public async Task<Result<ProductResponseDTO>> UpdateProductAsync(Guid id, UpdateDetailsProductDTO productDto, CancellationToken cancellationToken)
        {
            var product = await _productRepository.GetByIdAsync(id, cancellationToken);
            if (product == null)
            {
                return Result.Fail("Product not found.");
            }
            else
            {
                product.UpdateDetails(productDto.Name, productDto.Description, productDto.ThumbnailUrl);
                product.ChangePrice(productDto.Price);
                await _unitOfWork.CommitAsync(cancellationToken);
                return Result.Ok(MapProductToProductResponseDTO(product));
            }
        }

        private static ProductResponseDTO MapProductToProductResponseDTO(Product product)
        {
            return new ProductResponseDTO
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Price = product.BasePrice ?? 0,
                ThumbnailUrl = BuildAbsoluteThumbnailUrl(product.ThumbnailUrl),
                CategoryId = product.CategoryId
            };
        }

        private static string BuildAbsoluteThumbnailUrl(string? thumbnailUrl)
        {
            if (string.IsNullOrWhiteSpace(thumbnailUrl))
                return string.Empty;

            if (Uri.TryCreate(thumbnailUrl, UriKind.Absolute, out _))
                return thumbnailUrl;

            return $"{BaseUri}{(thumbnailUrl.StartsWith('/') ? thumbnailUrl : "/" + thumbnailUrl)}";
        }

        public async Task<Result<PageResponse<ProductResponseDTO>>> GetProductsByListCategoryIdAsync(List<Guid> categoryIds, int pageIndex, int pageSize, CancellationToken cancellationToken)
        {
            var specification = new ProductFilterSpecification(null, null, categoryIds, pageIndex: pageIndex, pageSize: pageSize);
            var root = await _productRepository.PagedListAsync(specification, pageIndex, pageSize, cancellationToken);
            var products = root.Items
                .Select(MapProductToProductResponseDTO)
                .ToList();
            var response = new PageResponse<ProductResponseDTO>(products, root.Total, pageSize, pageIndex);
            return Result.Ok(response);
        }
    }
}
