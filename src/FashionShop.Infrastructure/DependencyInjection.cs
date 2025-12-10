using FashionShop.Domain.Interfaces;
using FashionShop.Infrastructure.Repositories;
using FashionShop.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;
using FashionShop.Application.Common.Interfaces;


namespace FashionShop.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
        {
            services.AddScoped<IProductRepository, ProductRepository>();
            services.AddScoped<ICategoryRepository, CategoryRepository>();
            services.AddScoped<ISystemConfigRepository, SystemConfigsRepository>();
            services.AddScoped<IAIGenerationService, GeminiService>();
            return services;
        }
    }
}
