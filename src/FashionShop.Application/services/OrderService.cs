using FashionShop.Application.Common.Interfaces;
using FashionShop.Application.Interfaces;
using FashionShop.Domain.Common.Interfaces;
using FashionShop.Domain.Dto;
using FashionShop.Domain.Entities;
using FashionShop.Domain.Interfaces;
using FashionShop.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace FashionShop.Infrastructure.Services
{
    public class OrderService : IOrderService
    {
        private readonly IUserContext _userContext;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICartItemRepository _cartItemRepository;
        private readonly ICartRepository _cartRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly IVariantRepository _variantRepository;

        public OrderService(
            ICartItemRepository cartItemRepository,
            IApplicationDbContext dbContext, 
            IUserContext userContext, 
            IVariantRepository variantRepository,
            IOrderRepository orderRepository,
            ICartRepository cartRepository,
            IUnitOfWork unitOfWork)
        {
            _orderRepository = orderRepository;
            _variantRepository = variantRepository;
            _cartItemRepository = cartItemRepository;
            _cartRepository = cartRepository;
            _userContext = userContext;
            _unitOfWork = unitOfWork;
        }
        public Task CancelOderAsync(Guid orderId)
        {
            throw new NotImplementedException();
        }

        public async Task<Guid> CreateOrderAsync(OrderInputDto orderInput , CancellationToken cancellationToken)
        {
            Guid orderId = Guid.Empty;
            await _unitOfWork.ExecuteTransactionAsync(async () =>
            {
                var variantIds = orderInput.OrderItems.Select(oi => oi.VariantId).Distinct().ToList();

                var variants = await _variantRepository.GetByIdWithProductAsync(variantIds, cancellationToken);
                var newOrderItems = new List<OrderItem>();
                decimal totalAmount = 0;

                foreach (var itemRequest in orderInput.OrderItems)
                {
                    var dbVariant = variants.FirstOrDefault(v => v.Id == itemRequest.VariantId);

                    if (dbVariant == null)
                    {
                        throw new Exception($"Sản phẩm với ID {itemRequest.VariantId} không tồn tại.");
                    }

                    

                    var isStockReserved = await _variantRepository.DecreaseStockAsync(itemRequest.ProductId, itemRequest.VariantId, itemRequest.Quantity, cancellationToken);
                    if (!isStockReserved)
                    {
                        throw new Exception($"Sản phẩm '{dbVariant.Product.Name} - {dbVariant.Color}/{dbVariant.Size}' không đủ hàng. Chỉ còn {dbVariant.StockQuantity}.");
                    }
                    var orderItem = new OrderItem
                    {
                        Id = Guid.NewGuid(),
                        ProductVariantId = dbVariant.Id,
                        ProductId = dbVariant.ProductId,
                        ProductName = dbVariant.Product.Name, 
                        Color = dbVariant.Color,           
                        Size = dbVariant.Size,
                        UnitPrice = dbVariant.Price,        
                        Quantity = itemRequest.Quantity
                    };

                    newOrderItems.Add(orderItem);
                    totalAmount += orderItem.UnitPrice * orderItem.Quantity;
                }
                

                var order = new Order
                {
                    Id = Guid.NewGuid(),
                    UserId = _userContext.UserId,

                    Status = OrderStatus.Pending, 
                    TotalAmount = totalAmount,
                    ShippingAddress = new Address
                    {
                        StreetLine = orderInput.ShippingAddress.StreetLine,
                        City = orderInput.ShippingAddress.City,
                        Ward = orderInput.ShippingAddress.Ward,
                        District = orderInput.ShippingAddress.District,
                        FullNameAddress = orderInput.ShippingAddress.FullNameAddress
                    }, 
                    PhoneNumber = orderInput.PhoneNumber,
                    Note = orderInput.Note,
                    OrderItems = newOrderItems 
                };

                _orderRepository.Add(order);
                orderId = order.Id;



                var IsRemoved = await _cartItemRepository.ClearPurchasedItemsAsync(_userContext.UserId!.Value, variantIds, cancellationToken);
                await _unitOfWork.CommitAsync(cancellationToken);
               


            }, cancellationToken);
            return orderId;
        }
            

        public async Task<OrderDetailDto?> GetOderByIdAsync(Guid orderId, CancellationToken cancellationToken)
        {
            var Order = await _orderRepository.GetByIdWithItemsAsync(orderId, cancellationToken);
            if (Order == null) return null;
            var orderDetailDto = new OrderDetailDto
            {
                Id = Order.Id,
                CreatedAt = Order.CreatedAt,
                Status = Order.Status.ToString(),
                TotalAmount = Order.TotalAmount,
                ShippingAddress = Order.ShippingAddress,
                Items = [.. Order.OrderItems.Select(oi => new OrderLineItemDto
                {
                    ProductName = oi.ProductName,
                    Color = oi.Color,
                    Size = oi.Size,
                    UnitPrice = oi.UnitPrice,
                    Quantity = oi.Quantity
                })]
            };
            return orderDetailDto;
        }

        public async Task<List<OrderDetailDto>?> GetOderByUserAync(Guid userId, CancellationToken cancellationToken)
        {
            var Orders = await _orderRepository.GetByUserIdWithItemsAsync(userId, cancellationToken);
            if (Orders == null) return null;
            var orderDetailDtoList = Orders.Select(Order => new OrderDetailDto
            {
                Id = Order.Id,
                CreatedAt = Order.CreatedAt,
                Status = Order.Status.ToString(),
                TotalAmount = Order.TotalAmount,
                ShippingAddress = Order.ShippingAddress,
                Items = [.. Order.OrderItems.Select(oi => new OrderLineItemDto
                {
                    ProductName = oi.ProductName,
                    Color = oi.Color,
                    Size = oi.Size,
                    UnitPrice = oi.UnitPrice,
                    Quantity = oi.Quantity
                })]
            }).ToList();
            return orderDetailDtoList;
        }

        public async Task<OrderPreviewDto> PreviewOderAsync(OrderInputDto orderInput, CancellationToken cancellationToken)
        {
            var variantIds = orderInput.OrderItems.Select(oi => oi.VariantId).Distinct().ToList();
            var variants = await _variantRepository.GetByIdWithProductAsync(variantIds, cancellationToken);
            var newOrderLineItems = new List<OrderLineItemDto>();
            decimal totalAmount = 0;
            foreach (var item in orderInput.OrderItems)
            {
                var dbVariant = variants.FirstOrDefault(v => v.Id == item.VariantId);
                if(dbVariant == null)
                {
                    throw new Exception($"Sản phẩm với ID {item.VariantId} không tồn tại.");
                }
                if(dbVariant.StockQuantity < item.Quantity)
                {
                    throw new Exception($"Sản phẩm '{dbVariant.Product.Name} - {dbVariant.Color}/{dbVariant.Size}' không đủ hàng. Chỉ còn {dbVariant.StockQuantity}.");
                }
                OrderLineItemDto orderLineItem = new OrderLineItemDto
                {
                    ProductId = dbVariant.ProductId,
                    VariantId = dbVariant.Id,
                    ProductName = dbVariant.Product.Name,
                    Color = dbVariant.Color,
                    Size = dbVariant.Size,
                    UnitPrice = dbVariant.Price,
                    Quantity = item.Quantity,
                    ThumbnailUrl = dbVariant.ThumbnailUrl
                };
                newOrderLineItems.Add(orderLineItem);
                totalAmount += orderLineItem.UnitPrice * orderLineItem.Quantity;

            }
            var orderPreviewDto = new OrderPreviewDto
            {
                Subtotal = totalAmount,
                ShippingFee = 0,
                TotalAmount = totalAmount,
                OrderItems = newOrderLineItems
            };
            return orderPreviewDto;
        }

        public async Task UpdateOderStatusAsync(Guid orderId, string status, CancellationToken cancellationToken)
        {
            var order = await _orderRepository.GetByIdAsync(orderId, cancellationToken);
            if (order == null)
            {
                throw new Exception("Đơn hàng không tồn tại.");
            }
            order.Status = Enum.Parse<OrderStatus>(status);
            await _unitOfWork.CommitAsync(cancellationToken);
        }
    }
}
