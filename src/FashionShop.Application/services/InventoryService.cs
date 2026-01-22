using FashionShop.Application.Interfaces;
using FashionShop.Domain.Dto;
using FashionShop.Domain.Entities;
using FashionShop.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FashionShop.Infrastructure.Services
{
    public class InventoryService : IInventoryService
    {

        private readonly IVariantRepository _variantRepository;

        private readonly IUnitOfWork _unitOfWork;

        public InventoryService(
            IUnitOfWork unitOfWork, 
            IVariantRepository variantRepository)

        {

            _variantRepository = variantRepository;
            _unitOfWork = unitOfWork;

        }
        public async Task<(bool IsAvailable, string Message)> HasEnoughStockAsync(CartItemInputDto cartItem, CancellationToken cancellationToken)
        {
            var productVariant = await _variantRepository.GetByIdAsync(cartItem.VariantId, cancellationToken);
            if(productVariant == null)
            {
                return (false, "The variant does not exist.");
            }
            if (productVariant.ProductId != cartItem.ProductId)
            {
                return (false, $"Product variant with id {cartItem.VariantId} not found for product id {cartItem.ProductId}.");
            }
            if (productVariant.StockQuantity < cartItem.Quantity)
            {
                return (false, $"Only {productVariant!.StockQuantity} products of this model remain.");
            }
            return (true, "In stock");

        }

        public async Task<(bool IsAvailable, string Message)> ReduceStockAsync(CartItemInputDto cartItem, CancellationToken cancellationToken)
        {
            var productVariant = await _variantRepository.GetByIdAsync(cartItem.VariantId, cancellationToken);
            if (productVariant == null)
            {
                return (false, "The variant does not exist.");
            }
            if (productVariant.ProductId != cartItem.ProductId)
            {
                return (false, $"Product variant with id {cartItem.VariantId} not found for product id {cartItem.ProductId}.");
            }
            
            productVariant.StockQuantity -= cartItem.Quantity;
            await _unitOfWork.CommitAsync(cancellationToken);
            return (true, "Stock reduced successfully.");
        }

        public async Task<(bool IsAvailable, string Message )> RestoreStockAsync(CartItemInputDto cartItem, CancellationToken cancellationToken)
        {
            var productVariant = await _variantRepository.GetByIdAsync(cartItem.VariantId, cancellationToken);
            if (productVariant == null)
            {
                return (false, "The variant does not exist.");
            }
            if (productVariant.ProductId != cartItem.ProductId)
            {
                return (false, $"Product variant with id {cartItem.VariantId} not found for product id {cartItem.ProductId}.");
            }   
            
            productVariant.StockQuantity += cartItem.Quantity;
            await _unitOfWork.CommitAsync(cancellationToken);
            return (true, "Stock restored successfully.");
        }
    }
}
