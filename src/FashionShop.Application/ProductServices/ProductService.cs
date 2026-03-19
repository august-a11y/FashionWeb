using FashionShop.Application.Interfaces;
using FashionShop.Application.ProductServices.DTO;
using FashionShop.Domain.Entities;
using FluentResults;

namespace FashionShop.Application.ProductServices
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepository;
        private readonly IUnitOfWork _unitOfWork;
        public ProductService(IProductRepository productRepository, IUnitOfWork unitOfWork)
        {
            _productRepository = productRepository;
            _unitOfWork = unitOfWork;
        }
        public async Task<ProductResponseDTO> CreateProductAsync(CreateProductDTO productDto, CancellationToken cancellationToken)
        {
            var product = new Product
            {
                Name = productDto.Name,
                Description = productDto.Description,
                BasePrice = productDto.BasePrice,
                ThumbnailUrl = productDto.ThumbnailUrl,
                CategoryId = productDto.CategoryId
            };
            _productRepository.Add(product);
            await _unitOfWork.CommitAsync(cancellationToken);
            return new ProductResponseDTO
            {
                Name = product.Name,
                Description = product.Description,
                Price = product.BasePrice ?? 0,
                ThumbnailUrl = product.ThumbnailUrl,
                CategoryId = product.CategoryId
            };
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
                _productRepository.Remove(product);
                await _unitOfWork.CommitAsync(cancellationToken);
                return Result.Ok();
            }
           
        }

        public async Task<Result<IEnumerable<ProductResponseDTO>>> GetAllProductAsync(CancellationToken cancellationToken)
        {
            try
            {
                var products = await _productRepository.GetAllAsync(cancellationToken);


                return Result.Ok(products.Select(p => new ProductResponseDTO
                {
                    Name = p.Name,
                    Description = p.Description,
                    Price = p.BasePrice ?? 0,
                    ThumbnailUrl = p.ThumbnailUrl,
                    CategoryId = p.CategoryId
                }));
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
                return Result.Ok( new ProductResponseDTO
                {
                    Name = product.Name,
                    Description = product.Description,
                    Price = product.BasePrice ?? 0,
                    ThumbnailUrl = product.ThumbnailUrl,
                    CategoryId = product.CategoryId
                });
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
                return Result.Ok(new ProductResponseDTO
                {
                    Name = product.Name,
                    Description = product.Description,
                    Price = product.BasePrice ?? 0,
                    ThumbnailUrl = product.ThumbnailUrl,
                    CategoryId = product.CategoryId
                });
            }
        }
    }
}
