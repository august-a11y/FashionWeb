using FashionShop.API;
using FashionShop.API.Middleware;
using FashionShop.Application;
using FashionShop.Domain.Identity;
using FashionShop.Infrastructure;
using FashionShop.Infrastructure.ConfigOptions;
using FashionShop.Infrastructure.Persistence;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;
using StackExchange.Redis;
using System.Text;
using System.Text.Json.Serialization;


internal class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("Missing configuration 'ConnectionStrings:DefaultConnection'.");
        }

        var redisConnectionString = builder.Configuration.GetConnectionString("Redis");
        if (string.IsNullOrWhiteSpace(redisConnectionString))
        {
            throw new InvalidOperationException("Missing configuration 'ConnectionStrings:Redis'.");
        }

        
        
        var jwtKey = builder.Configuration["Jwt:Key"];
        var jwtIssuer = builder.Configuration["Jwt:Issuer"];
        var jwtAudience = builder.Configuration["Jwt:Audience"];
        if (string.IsNullOrWhiteSpace(jwtKey))
        {
            throw new InvalidOperationException("Missing configuration 'Jwt:Key'. Set it in appsettings or environment variable 'Jwt__Key'.");
        }


        if (string.IsNullOrWhiteSpace(jwtIssuer))
        {
            throw new InvalidOperationException("Missing configuration 'Jwt:Issuer'.");
        }


        if (string.IsNullOrWhiteSpace(jwtAudience))
        {
            throw new InvalidOperationException("Missing configuration 'Jwt:Audience'.");
        }

        var localhostRabbitMqHost = builder.Configuration["RabbitMq:Host"];
        var localhostRabbitMqUsername = builder.Configuration["RabbitMq:Username"];
        var localhostRabbitMqPassword = builder.Configuration["RabbitMq:Password"];
        var key = Encoding.UTF8.GetBytes(jwtKey);

        builder.Services.AddSingleton<IConnectionMultiplexer>(_ =>
            ConnectionMultiplexer.Connect(redisConnectionString));
        builder.Services.AddMassTransit(x =>
        {
            x.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host(localhostRabbitMqHost, "/", h =>
                {
                    h.Username(localhostRabbitMqUsername); 
                    h.Password(localhostRabbitMqPassword);
                });
            });
        });

        builder.Services.AddCors(options =>
        {
            options.AddPolicy("CorsPolicy", policy =>
            {
                policy.WithOrigins("https://localhost:4200")
                      .AllowAnyHeader()
                      .AllowAnyMethod()
                      .AllowCredentials();
            });
        });

        builder.Services.AddDbContextFactory<ApplicationDbContext>(options =>
        {
            options.UseSqlServer(connectionString, b =>
            {
                b.MigrationsAssembly("FashionShop.Infrastructure");
                b.UseCompatibilityLevel(120);
            });
        });

        builder.Services.AddIdentity<AppUser, AppRole>(options => options.SignIn.RequireConfirmedAccount = false)
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

        builder.Services.Configure<IdentityOptions>(options =>
        {
            options.Password.RequireDigit = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequireUppercase = true;
            options.Password.RequiredLength = 6;
            options.Password.RequiredUniqueChars = 1;

            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
            options.Lockout.MaxFailedAccessAttempts = 5;
            options.Lockout.AllowedForNewUsers = true;

            options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
            options.User.RequireUniqueEmail = false;
        });

        builder.Services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.SaveToken = true;
            options.RequireHttpsMetadata = false;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtIssuer,
                ValidAudience = jwtAudience,
                IssuerSigningKey = new SymmetricSecurityKey(key)
            };
        });

        builder.Services.Configure<JwtTokenSettings>(jwtSettings);

        builder.Services.AddAuthorization(options =>
        {
            options.AddPolicy("Admin", policy => policy.RequireRole("Admin"));
            options.AddPolicy("Custommer", policy => policy.RequireRole("Custommer"));
        });

        builder.Services.AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            });
        builder.Services.AddApplicationServices();
        builder.Services.AddInfrastructureServices();

        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        var app = builder.Build();

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();

        var contentTypeProvider = new FileExtensionContentTypeProvider();
        var imgPath = Path.Combine(app.Environment.ContentRootPath, "img");
        Directory.CreateDirectory(imgPath);

        app.UseStaticFiles(new StaticFileOptions
        {
            FileProvider = new PhysicalFileProvider(imgPath),
            RequestPath = "/img",
            ContentTypeProvider = contentTypeProvider,
            OnPrepareResponse = ctx =>
            {
                ctx.Context.Response.Headers["Access-Control-Allow-Origin"] = "https://localhost:4200";
            }
        });

        app.UseMiddleware<SessionMiddleware>();
        app.UseMiddleware<JwtContextMiddleware>();

        app.UseCors("CorsPolicy");

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers();

        app.MigrationDatabase();

        app.Run();
    }
}