using FashionShop.Application.Auth;
using FashionShop.Application.CartService;
using FashionShop.Application.CategoryService;
using FashionShop.Application.Common.Interfaces;
using FashionShop.Application.OrderService;
using FashionShop.Application.ProductService;
using FashionShop.Application.ProductService.Validation;
using FashionShop.Domain.Interfaces;
using FashionShop.Infrastructure.Cache;
using FashionShop.Infrastructure.Caching;
using FashionShop.Infrastructure.Persistence;
using FashionShop.Infrastructure.Persistence.Repositories;
using FashionShop.Infrastructure.Services;

using FluentValidation;
using Microsoft.Extensions.DependencyInjection;


namespace FashionShop.Infrastructure
{
    public static class DependencyInjection
    {
     
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
        {
  
            //DI services
            services.AddScoped<IOrderService, OrderService>();
            services.AddScoped<IProductService, ProductService>();
            services.AddScoped<ICategoryService, CategoryService>();
            services.AddScoped<ICartService, CartService>();
            services.AddScoped<IAuthenticatedService, AuthenticatedService>();
            services.AddScoped<IJwtService, JwtService>();

            //DI repositories
            services.AddScoped<IOrderRepository, OrderRepository>();
            services.AddScoped<ICartItemRepository, CartItemRepository>();
            services.AddScoped<IVariantRepository, VariantRepository>();
            services.AddScoped<IProductRepository, ProductRepository>();
            services.AddScoped<ICartRepository, CartRepository>();
            services.AddScoped<ICartCacheRepository, CartCacheRepository>();

            services.AddScoped(typeof(IRepository<,>), typeof(Repository<,>));
            services.AddMemoryCache();
            services.AddScoped<HttpClient>();
            services.AddValidatorsFromAssembly(typeof(ProductValidator).Assembly);
            services.AddValidatorsFromAssembly(typeof(CategoryValidation).Assembly);
            services.AddScoped(typeof(IApplicationDbContext), typeof(ApplicationDbContext));
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<RequestContext>();
            services.AddScoped<IRequestContext>(sp => sp.GetRequiredService<RequestContext>());
            services.AddScoped<IRedisCache, RedisCache>();

            return services;
        }
        
    }
}
