using FashionShop.Application.Common.Interfaces;
using FashionShop.Application.Interfaces;
using FashionShop.Infrastructure.Cache;
using FashionShop.Infrastructure.Caching;
using FashionShop.Infrastructure.Persistence;
using FashionShop.Infrastructure.Persistence.Repositories;
using FashionShop.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;
using FashionShop.Infrastructure.Identity;
using FashionShop.Application.Services.AuthServices;
using FashionShop.Application.Services.DashboardServices;
using FashionShop.Application.Services;

namespace FashionShop.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
        {
            // Infrastructure services
            services.AddScoped<IAuthenticatedService, AuthenticatedService>();
            services.AddScoped<IJwtService, JwtService>();
            services.AddScoped<IRedisCache, RedisCache>();
            services.AddScoped<IIdentityService, IdentityService>();
            services.AddScoped<IPhotoService, LocalPhotoService>();
            services.AddScoped<IDashboardService, DashboardService>();
            services.AddScoped<IDashboardReadRepository, DashboardReadRepository>();

            // Repositories
            services.AddScoped<ICartItemRepository, CartItemRepository>();
            services.AddScoped<IVariantRepository, VariantRepository>();
            services.AddScoped<ICartRepository, CartRepository>();
            services.AddScoped<ICartCacheRepository, CartCacheRepository>();
            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

            // Db + context
            services.AddScoped(typeof(IApplicationDbContext), typeof(ApplicationDbContext));
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<RequestContext>();
            services.AddScoped<IRequestContext>(sp => sp.GetRequiredService<RequestContext>());


            // Shared infra utilities
            services.AddMemoryCache();
            services.AddScoped<HttpClient>();

            return services;
        }
    }
}
