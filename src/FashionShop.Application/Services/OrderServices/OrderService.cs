using FashionShop.Application.Common.Interfaces;
using FashionShop.Application.Events;
using FashionShop.Application.Interfaces;
using FashionShop.Application.Services.AddressServices.DTO;
using FashionShop.Application.Services.OrderServices.DTO;
using FashionShop.Application.Specifications;
using FashionShop.Domain.Entities;
using FluentResults;

namespace FashionShop.Application.Services.OrderServices
{
    public class OrderService : IOrderService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepository<Order> _orderRepository;
        private readonly IVariantRepository _variantRepository;
        private readonly ICartItemRepository _cartItemRepository;
        private readonly IRequestContext _userContext;
        private readonly IMessagePublisher _messagePublisher;

        public OrderService(
            IUnitOfWork unitOfWork,
            IRepository<Order> orderRepository,
            IVariantRepository variantRepository,
            ICartItemRepository cartItemRepository,
            IMessagePublisher messagePublisher,
            IRequestContext userContext)
        {
            _unitOfWork = unitOfWork;
            _orderRepository = orderRepository;
            _variantRepository = variantRepository;
            _cartItemRepository = cartItemRepository;
            _userContext = userContext;
            _messagePublisher = messagePublisher;
        }

        public async Task<Result<OrderDTO>> CreateOrderAsync(CreateOrderDTO createOrderDto, CancellationToken cancellationToken)
        {
            if (createOrderDto.OrderItems == null || createOrderDto.OrderItems.Count == 0)
                return Result.Fail<OrderDTO>("Order must contain at least one item.");

            //if (!_userContext.UserId.HasValue)
            //    return Result.Fail<OrderDTO>("User is not authenticated.");

            var variantIds = createOrderDto.OrderItems
                .Select(x => x.VariantId)
                .Distinct()
                .ToList();

            var variants = await _variantRepository.GetListByIdsWithProductAsync(variantIds, cancellationToken);
            var variantMap = variants.ToDictionary(v => v.Id);

            var newOrderItems = new List<OrderItem>();
            decimal subTotal = 0m;

            foreach (var itemRequest in createOrderDto.OrderItems)
            {
                if (itemRequest.Quantity <= 0)
                    return Result.Fail<OrderDTO>("Quantity must be greater than 0.");

                if (!variantMap.TryGetValue(itemRequest.VariantId, out var dbVariant))
                    return Result.Fail<OrderDTO>($"Variant '{itemRequest.VariantId}' does not exist.");

                var isStockReserved = await _variantRepository.DecreaseStockAsync(
                    dbVariant.ProductId,
                    dbVariant.Id,
                    itemRequest.Quantity,
                    cancellationToken);

                if (!isStockReserved)
                {
                    return Result.Fail<OrderDTO>(
                        $"Product '{dbVariant.Product.Name} - {dbVariant.Color}/{dbVariant.Size}' is out of stock. Only {dbVariant.StockQuantity} left.");
                }

                var orderItem = new OrderItem
                {
                    ProductId = dbVariant.ProductId,
                    VariantId = dbVariant.Id,
                    ProductName = dbVariant.Product.Name,   
                    Color = dbVariant.Color,                
                    Size = dbVariant.Size,                  
                    ThumbnailUrl = dbVariant.ThumbnailUrl,  
                    Quantity = itemRequest.Quantity,
                    UnitPrice = dbVariant.Price
                };

                newOrderItems.Add(orderItem);
                subTotal += orderItem.LineTotal;
            }

            var shippingFee = 0m;
            var totalAmount = subTotal + shippingFee;

            var order = new Order
            {
                OrderCode = GenerateOrderCode(),
                UserId = _userContext.UserId.HasValue ? _userContext.UserId.Value : null,
                Status = OrderStatus.Pending,
                SubTotal = subTotal,
                ShippingFee = shippingFee,
                TotalAmount = totalAmount,
                PhoneNumber = createOrderDto.PhoneNumber,
                Note = createOrderDto.Note,
                ShippingAddress = new OrderAddressSnapshot
                {
                    RecipientName = createOrderDto.ShippingAddress.FullName,
                    RecipientPhone = createOrderDto.ShippingAddress.PhoneNumber,
                    RecipientEmail = createOrderDto.ShippingAddress.Email,
                    StreetAddress = createOrderDto.ShippingAddress.StreetLine,
                    Ward = createOrderDto.ShippingAddress.Ward,
                    District = createOrderDto.ShippingAddress.District,
                    City = createOrderDto.ShippingAddress.City
                },
                Payment = new Payment
                {
                    PaymentMethod = createOrderDto.PaymentMethod,
                    Amount = totalAmount,
                    Status = PaymentStatus.Pending,
                    TransactionId = string.Empty
                },
                Shipment = new Shipment
                {
                    Status = ShipmentStatus.Preparing,
                    Carrier = string.Empty,
                    TrackingNumber = string.Empty
                },
                OrderItems = newOrderItems
            };

            Order? createdOrder = null;

            await _unitOfWork.ExecuteTransactionAsync(async () =>
            {
                await _orderRepository.AddAsync(order, cancellationToken);
                createdOrder = order;

                if (_userContext.UserId.HasValue)
                {
                    await _cartItemRepository.DeleteItemsByCartIdAsync(
                    _userContext.UserId.Value,
                    variantIds,
                    cancellationToken);
                }
                

                await _unitOfWork.CommitAsync(cancellationToken);
            }, cancellationToken);

            if (createdOrder == null)
                return Result.Fail<OrderDTO>("Unable to create order.");
            var orderEvent = new OrderCreatedEvent(order.OrderCode, order.ShippingAddress.RecipientEmail, order.ShippingAddress.RecipientName, order.ShippingAddress.RecipientPhone, order.ShippingAddress.StreetAddress, order.Status.ToString(), order.TotalAmount, order.CreatedAt);
            await _messagePublisher.PublishAsync(orderEvent);

            return Result.Ok(MapOrderToDto(createdOrder));
        }

        public async Task<Result<OrderDTO>> GetOrderByIdAsync(Guid orderId, CancellationToken cancellationToken)
        {
            var spec = new OrderWithItemsByIdSpec(orderId);
            var order = await _orderRepository.FirstOrDefaultAsync(spec, cancellationToken);
            if (order == null)
                return Result.Fail<OrderDTO>("Order not found.");

            return Result.Ok(MapOrderToDto(order));
        }

        public async Task<Result<List<OrderDTO>>> GetOrdersByUserIdAsync(Guid userId, CancellationToken cancellationToken)
        {
            var spec = new OrdersWithItemsByUserIdSpec(userId);
            var orders = await _orderRepository.ListAsync(spec, cancellationToken);
            if (orders == null || orders.Count == 0)
                return Result.Ok(new List<OrderDTO>());

            return Result.Ok(orders.Select(MapOrderToDto).ToList());
        }

        public async Task<Result<bool>> CancelOrderAsync(Guid orderId, CancellationToken cancellationToken)
        {
            var order = await _orderRepository.GetByIdAsync(orderId, cancellationToken);
            if (order == null)
                return Result.Fail<bool>("Order does not exist.");

            if (order.Status is OrderStatus.Delivered or OrderStatus.Cancelled)
                return Result.Fail<bool>("Order cannot be cancelled.");

            order.Status = OrderStatus.Cancelled;
            await _unitOfWork.CommitAsync(cancellationToken);

            return Result.Ok(true);
        }

        public async Task<Result<OrderPreviewDto>> PreviewOderAsync(CreateOrderDTO orderInput, CancellationToken cancellationToken)
        {
            if (orderInput.OrderItems == null || orderInput.OrderItems.Count == 0)
                return Result.Fail<OrderPreviewDto>("Order must contain at least one item.");

            var variantIds = orderInput.OrderItems.Select(oi => oi.VariantId).Distinct().ToList();
            var variants = await _variantRepository.GetListByIdsWithProductAsync(variantIds, cancellationToken);
            var variantMap = variants.ToDictionary(v => v.Id);

            var previewItems = new List<OrderItemDTO>();
            decimal subtotal = 0m;

            foreach (var item in orderInput.OrderItems)
            {
                if (item.Quantity <= 0)
                    return Result.Fail<OrderPreviewDto>("Quantity must be greater than 0.");

                if (!variantMap.TryGetValue(item.VariantId, out var dbVariant))
                    return Result.Fail<OrderPreviewDto>($"Variant '{item.VariantId}' does not exist.");

                if (dbVariant.StockQuantity < item.Quantity)
                    return Result.Fail<OrderPreviewDto>(
                        $"Product '{dbVariant.Product.Name} - {dbVariant.Color}/{dbVariant.Size}' is out of stock. Only {dbVariant.StockQuantity} left.");

                previewItems.Add(new OrderItemDTO
                {
                    ProductId = dbVariant.ProductId,
                    VariantId = dbVariant.Id,
                    ProductName = dbVariant.Product.Name,
                    Color = dbVariant.Color,
                    Size = dbVariant.Size,
                    ThumbnailUrl = dbVariant.ThumbnailUrl,
                    UnitPrice = dbVariant.Price,
                    Quantity = item.Quantity
                });

                subtotal += dbVariant.Price * item.Quantity;
            }

            return Result.Ok(new OrderPreviewDto
            {
                Subtotal = subtotal,
                ShippingFee = 0m,
                TotalAmount = subtotal,
                OrderItems = previewItems
            });
        }

        public async Task<Result<bool>> UpdateOderStatusAsync(Guid orderId, OrderStatus status, CancellationToken cancellationToken)
        {
            var order = await _orderRepository.GetByIdAsync(orderId, cancellationToken);
            if (order == null)
                return Result.Fail<bool>("Order does not exist.");

            var currentStatus = order.Status;
            var newStatus = status;

            if (currentStatus == newStatus)
                return Result.Ok(true);

            if (!IsValidStatusTransition(currentStatus, newStatus))
                return Result.Fail<bool>($"Invalid status transition: {currentStatus} -> {newStatus}.");

            order.Status = newStatus;
            await _unitOfWork.CommitAsync(cancellationToken);

            return Result.Ok(true);
        }

        private static OrderDTO MapOrderToDto(Order order)
        {
            return new OrderDTO
            {
                Id = order.Id,
                OrderCode = string.IsNullOrWhiteSpace(order.OrderCode)
                    ? order.Id.ToString("N").ToUpper()[..10]
                    : order.OrderCode,
                CreatedAt = order.CreatedAt,
                ExpectedDeliveryDate = order.ExpectedDeliveryDate,
                SubTotal = order.SubTotal,
                ShippingFee = order.ShippingFee,
                GrandTotal = order.TotalAmount,
                Note = order.Note,
                PaymentMethod = order.Payment?.PaymentMethod ?? PaymentMethod.COD,
                PaymentStatus = order.Payment?.Status ?? PaymentStatus.Pending,
                ShippingStatus = order.Shipment?.Status ?? ShipmentStatus.Preparing,
                Status = order.Status,
                Items = order.OrderItems.Select(oi => new OrderItemDTO
                {
                    ProductId = oi.ProductId,
                    VariantId = oi.VariantId,
                    ProductName = oi.ProductName,      
                    Color = oi.Color,                  
                    Size = oi.Size,                    
                    ThumbnailUrl = oi.ThumbnailUrl,    
                    Quantity = oi.Quantity,
                    UnitPrice = oi.UnitPrice
                }).ToList(),
                ShippingAddress = new AddressDTO
                {
                    FullName = order.ShippingAddress?.RecipientName ?? string.Empty,
                    PhoneNumber = order.ShippingAddress?.RecipientPhone ?? string.Empty,
                    StreetLine = order.ShippingAddress?.StreetAddress ?? string.Empty,
                    Ward = order.ShippingAddress?.Ward ?? string.Empty,
                    District = order.ShippingAddress?.District ?? string.Empty,
                    City = order.ShippingAddress?.City ?? string.Empty
                }
            };
        }

        private static string GenerateOrderCode()
        {
            return $"ORD-{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid():N}"[..32];
        }

        private static bool IsValidStatusTransition(OrderStatus from, OrderStatus to)
        {
            return from switch
            {
                OrderStatus.Pending => to is OrderStatus.Processing or OrderStatus.Cancelled,
                OrderStatus.Processing => to is OrderStatus.Shipping or OrderStatus.Cancelled,
                OrderStatus.Shipping => to == OrderStatus.Delivered,
                OrderStatus.Delivered => false,
                OrderStatus.Cancelled => false,
                _ => false
            };
        }

        public async Task<Result<List<OrderDTO>>> GetAllOrdersAsync(CancellationToken cancellationToken)
        {
            var order = await _orderRepository.ListAsync(cancellationToken);
            if (order == null || order.Count == 0)
                return Result.Ok(new List<OrderDTO>());
            var orderDtos = order.Select(MapOrderToDto).ToList();
            return Result.Ok(orderDtos);
        }
    }
}
