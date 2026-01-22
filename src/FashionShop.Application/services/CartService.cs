using FashionShop.Application.Common.Interfaces;
using FashionShop.Application.Interfaces;
using FashionShop.Domain.Common.Interfaces;
using FashionShop.Domain.Dto;
using FashionShop.Domain.Entities;
using FashionShop.Domain.Interfaces;


namespace FashionShop.Infrastructure.Services
{
    public class CartService : ICartService
    {

        private readonly IRedisService _redisService;
        private readonly ICartRepository _cartRepository;
        private readonly ICartItemRepository _cartItemRepo;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepository<Product, Guid> _productRepository;
        private readonly IVariantRepository _variantRepository;
        private readonly IUserContext _userContext;
        public CartService(
            IRedisService redisService, 
            ICartRepository repository,
            IUserContext userContext,
            IUnitOfWork unitOfWork, 
            IRepository<Product, Guid> productRepository,
            ICartItemRepository cartItemRepo,
            IVariantRepository variantRepository)
        {
            _cartItemRepo = cartItemRepo;
            _userContext = userContext;
            _unitOfWork = unitOfWork;
            _cartRepository = repository;
            _redisService = redisService;
            _productRepository = productRepository;
            _variantRepository = variantRepository;
        }
        public async Task AddToCartAsync(AddToCartDto cartItem, CancellationToken cancellationToken)
        {
            var variant =  await _variantRepository.GetByIdAsync(cartItem.VariantId, cancellationToken);
            if(variant == null)
            {
                throw new Exception("Product variant not found.");
            }
            if(cartItem.Quantity > variant.StockQuantity)
            {
                throw new Exception("Insufficient stock for the requested quantity.");
            }
            if (!_userContext.IsAuthenticated)
            {
                var key = $"cart:guest:{_userContext.SessionId}";
                var field = $"{cartItem.ProductId}_{cartItem.VariantId}";
                var existingItem = await _redisService.HashGetAsync<AddToCartDto>(key, field);


                if (existingItem != null)
                {
                    cartItem.Quantity += existingItem.Quantity;
                }


                await _redisService.HashSetAsync(key, field, cartItem);


                await _redisService.SetExpireAsync(key, TimeSpan.FromDays(30));
                return;


            }
                
            var userId = _userContext.UserId!.Value;
            var cart = await _cartRepository.GetCartWithItemsByUserIdAsync(userId, cancellationToken);
            if (cart == null)
            {
                cart = new Cart
                {
                    UserId = userId,
                    CartItems = new List<CartItem>()
                };
                _cartRepository.Add(cart);
                await _unitOfWork.CommitAsync(cancellationToken);
            }

            var existingCartItem = cart.CartItems.FirstOrDefault(i => i.ProductId == cartItem.ProductId && i.ProductVariantId == cartItem.VariantId);
            if (existingCartItem != null)
            {
                existingCartItem.Quantity += cartItem.Quantity;
            }
            else
            {
                _cartItemRepo.Add(new CartItem
                {
                    CartId = cart.Id,
                    ProductId = cartItem.ProductId,
                    ProductVariantId = cartItem.VariantId,
                    Quantity = cartItem.Quantity
                });
                await _unitOfWork.CommitAsync(cancellationToken);
            }
            
            await _redisService.DeleteKeyAsync($"cart:guest:{_userContext.SessionId}");
            


        }

        public async Task<CartOutputDto> GetCartAsync(CancellationToken cancellationToken)
        {
            var cartSkeleton = new List<(Guid ProductId, Guid VariantId, int Quantity)>();

            if (_userContext.IsAuthenticated)
            {
                
                var cart = await _cartRepository.GetCartWithItemsByUserIdAsync(_userContext.UserId!.Value, cancellationToken);

                if (cart != null)
                {
                    cartSkeleton = cart.CartItems.Select(x => (x.ProductId, x.ProductVariantId, x.Quantity)).ToList();
                }
            }
            else
            {
               
                var key = $"cart:guest:{_userContext.SessionId}";
                var redisItems = await _redisService.HashGetAllAsync<CartItemInputDto>(key); 

                foreach (var item in redisItems)
                {
                    cartSkeleton.Add((item.Value.ProductId, item.Value.VariantId, item.Value.Quantity));
                }
            }

            if (!cartSkeleton.Any()) return new CartOutputDto();

            var productIds = cartSkeleton.Select(x => x.ProductId).Distinct().ToList();
            var variantIds = cartSkeleton.Select(x => x.VariantId).Distinct().ToList();

            var productsTask = _productRepository.GetListByIdsAsync(productIds, cancellationToken);
            var variantsTask = _variantRepository.GetListByIdsAsync(variantIds, cancellationToken);

            await Task.WhenAll(productsTask, variantsTask); 

            var products = productsTask.Result;
            var variants = variantsTask.Result;

            var outputCart = new CartOutputDto();

            foreach (var item in cartSkeleton)
            {

                var product = products.FirstOrDefault(p => p.Id == item.ProductId);
                var variant = variants.FirstOrDefault(v => v.Id == item.VariantId);

                if (product == null || variant == null) continue;

                var cartItemDto = new CartItemDto
                {
                    ProductId = item.ProductId,
                    VariantId = item.VariantId,
                    Quantity = item.Quantity,
                    ProductName = product.Name,
                    Color = variant.Color, 
                    Size = variant.Size,
                    ThumbnailUrl = !string.IsNullOrEmpty(variant.ThumbnailUrl) ? variant.ThumbnailUrl : product.ThumbnailUrl,
                    Price = variant.Price, 
                    StockQuantity = variant.StockQuantity,
                    IsValid = variant.StockQuantity > item.Quantity,
                };

                outputCart.Items.Add(cartItemDto);
            }

            outputCart.TotalItems = outputCart.Items.Sum(x => x.Quantity);
            outputCart.TotalPrice = outputCart.Items.Sum(x => x.LineTotal);

            return outputCart;
        }

        public async Task MergeCartsAsync(string anonymousCartId, CancellationToken cancellationToken)
        {
            var guestKey = $"cart:guest:{anonymousCartId}";
            var guestItems = await _redisService.HashGetAllAsync<CartItemInputDto>(guestKey);

            if (guestItems == null || !guestItems.Any()) return; 

            var userCart = await _cartRepository.GetCartWithItemsByUserIdAsync(_userContext.UserId!.Value, cancellationToken);

            if (userCart == null)
            {
                userCart = new Cart
                {
                    UserId = _userContext.UserId.Value,
                    CartItems = new List<CartItem>()
                };
                _cartRepository.Add(userCart);
            }

            foreach (var guestItem in guestItems)
            {
                var ids = guestItem.Key.Split('_');
                var productId = Guid.Parse(ids[0]);
                var variantId = Guid.Parse(ids[1]);
                var quantity = guestItem.Value.Quantity;
                var existingItem = userCart.CartItems.FirstOrDefault(x =>
                    x.ProductId == productId &&
                    x.ProductVariantId == variantId);

                if (existingItem != null)
                {
                    existingItem.Quantity += quantity;
                }
                else
                {
                    userCart.CartItems.Add(new CartItem
                    {
                        ProductId = productId,
                        ProductVariantId = variantId,
                        Quantity = quantity
                    });
                }
            }

            await _unitOfWork.CommitAsync(cancellationToken);
            await _redisService.DeleteKeyAsync(guestKey);
            await _redisService.DeleteKeyAsync($"cart:user:{_userContext.UserId}");
        }

        public async Task ReduceQuantityOfItemAsync(CartItemInputDto cartItem, CancellationToken cancellationToken)
        {
            if (!_userContext.IsAuthenticated)
            {
                var key = $"cart:guest:{_userContext.SessionId}";
                var field = $"{cartItem.ProductId}_{cartItem.VariantId}";
                var existingItem = await _redisService.HashGetAsync<CartItemInputDto>(key, field);
                if (existingItem != null)
                {
                    existingItem.Quantity -= cartItem.Quantity;
                    if(existingItem.Quantity <= 0)
                    {
                        await _redisService.HashDeleteAsync(key, field);
                    }
                    else
                    {
                        await _redisService.HashSetAsync(key, field, existingItem);
                    }
                }
                
                return;
            }
            var cart = await _cartRepository.GetCartWithItemsByUserIdAsync(_userContext.UserId!.Value, cancellationToken);
            if (cart == null || !cart.CartItems.Any()) return;

            var itemToReduce = cart.CartItems.FirstOrDefault(i => i.ProductId == cartItem.ProductId && i.ProductVariantId == cartItem.VariantId);
            if (itemToReduce != null)
            {
                itemToReduce.Quantity -= cartItem.Quantity;
                if(itemToReduce.Quantity <= 0) cart.CartItems.Remove(itemToReduce);
            }
            await _unitOfWork.CommitAsync(cancellationToken);
            await _redisService.DeleteKeyAsync($"cart:user:{_userContext.UserId}");
        }

        public async Task RemoveItemAsync(Guid productId, Guid variantId, CancellationToken cancellationToken)
        {
            if (!_userContext.IsAuthenticated)
            {
                var key = $"cart:guest:{_userContext.SessionId}";
                var field = $"{productId}_{variantId}";
                await _redisService.HashDeleteAsync(key, field);
                return;
            }
            var cart = await _cartRepository.GetCartWithItemsByUserIdAsync(_userContext.UserId!.Value, cancellationToken);
            if (cart == null || !cart.CartItems.Any()) return;

            var itemToRemove = cart.CartItems.FirstOrDefault(i => i.ProductId == productId && i.ProductVariantId == variantId);
            if (itemToRemove != null)
            {
                cart.CartItems.Remove(itemToRemove);
                await _unitOfWork.CommitAsync(cancellationToken);
                await _redisService.DeleteKeyAsync($"cart:user:{_userContext.UserId}");
            }
        }
    }
}
