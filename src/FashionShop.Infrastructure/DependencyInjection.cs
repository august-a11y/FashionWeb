using FashionShop.Domain.Interfaces;
using FashionShop.Infrastructure.Repositories;
using Microsoft.Extensions.DependencyInjection;


namespace FashionShop.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
        {
            services.AddScoped<IProductRepository, ProductRepository>();
            services.AddScoped<ICategoryRepository, CategoryRepository>();
            services.AddScoped<ISystemConfigRepository, SystemConfigsRepository>();
            return services;
        }
    }
}
