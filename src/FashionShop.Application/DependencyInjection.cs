using FashionShop.Application.CartServices;
using FashionShop.Application.CategoryServices;
using FashionShop.Application.Common.Behaviors;
using FashionShop.Application.OrderServices;
using FashionShop.Application.ProductServices;
using FashionShop.Application.VariantServices;
using FashionShop.Application.AddressServices;
using FashionShop.Application.UserServices;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

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