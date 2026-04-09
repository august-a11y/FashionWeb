using FashionShop.Application.Common.Behaviors;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using FashionShop.Application.Services.ProductServices;
using FashionShop.Application.Services.OrderServices;
using FashionShop.Application.Services.VariantServices;
using FashionShop.Application.Services.UserServices;
using FashionShop.Application.Services.CategoryServices;
using FashionShop.Application.Services.AddressServices;
using FashionShop.Application.Services.CartServices;

namespace FashionShop.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            // Application services
            services.AddScoped<IOrderService, OrderService>();
            services.AddScoped<IProductService, ProductService>();
            services.AddScoped<ICategoryService, CategoryService>();
            services.AddScoped<ICartService, CartService>();
            services.AddScoped<IVariantService, VariantService>();
            services.AddScoped<IAddressService, AddressService>();
            services.AddScoped<IUserService, UserService>();

            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

            return services;
        }
    }
}