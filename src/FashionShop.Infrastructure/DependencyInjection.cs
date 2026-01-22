using FashionShop.Application.Categories;
using FashionShop.Application.Categories.Commands;
using FashionShop.Application.Common.Behaviors;
using FashionShop.Application.Common.Interfaces;
using FashionShop.Application.Common.Mappings;
using FashionShop.Application.Interfaces;
using FashionShop.Application.Products.Validation;
using FashionShop.Domain.Common.Interfaces;
using FashionShop.Domain.Interfaces;
using FashionShop.Infrastructure.Persistence;
using FashionShop.Infrastructure.Persistence.Repositories;
using FashionShop.Infrastructure.Services;

using FluentValidation;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;


namespace FashionShop.Infrastructure
{
    public static class DependencyInjection
    {
     
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
        {
            services.AddScoped(typeof(IRepository<,>), typeof(Repository<,>));
            services.AddScoped<IJwtService, JwtService>();
            services.AddScoped<ICartRepository, CartRepository>();
            services.AddMemoryCache(); 
            services.AddScoped<HttpClient>();
            services.AddAutoMapper(typeof(MappingProfile));
            services.AddValidatorsFromAssembly(typeof(ProductValidator).Assembly);
            services.AddValidatorsFromAssembly(typeof(CategoryValidation).Assembly);
            services.AddScoped<ICartService, CartService>();
            services.AddScoped(typeof(IApplicationDbContext), typeof(ApplicationDbContext));
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<IInventoryService, InventoryService>();
            services.AddScoped<IOrderService, OrderService>();
            services.AddScoped<IOrderRepository, OrderRepository>();
            services.AddScoped<ICartItemRepository, CartItemRepository>();
            services.AddScoped<IVariantRepository, VariantRepository>();
            services.AddScoped<UserContext>();
            services.AddScoped<IUserContext>(sp => sp.GetRequiredService<UserContext>());

            services.AddMediatR(cfg => {
                cfg.RegisterServicesFromAssembly(typeof(CreateCategoryCommandHandler).Assembly);
                cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
            });
            return services;
        }
        
    }
}
