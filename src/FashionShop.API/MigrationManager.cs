using FashionShop.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FashionShop.API
{
    public static class MigrationManager
    {
        public static WebApplication MigrationDatabase(this WebApplication app)
        {
     
            using (var scope = app.Services.CreateScope())
            {
                using(var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>())
                
                {
                    context.Database.Migrate();
                    new  DataSeeder().SeedAsync(context).Wait();

                }
                
            }
            return app;
        }
    }
}
