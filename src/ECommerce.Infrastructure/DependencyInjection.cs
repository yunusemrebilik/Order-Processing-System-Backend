using System.Text;
using ECommerce.Application.Common.Interfaces;
using ECommerce.Application.Common.Settings;
using ECommerce.Infrastructure.Data;
using ECommerce.Infrastructure.Repositories;
using ECommerce.Infrastructure.Services;
using ECommerce.Infrastructure.Settings;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using StackExchange.Redis;

namespace ECommerce.Infrastructure;

/// <summary>
/// Extension method to register all Infrastructure services.
/// Called from Program.cs to keep it clean.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration,
        Action<IBusRegistrationConfigurator>? configureBus = null,
        bool includeWebAuth = true)
    {
        // PostgreSQL + EF Core
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("PostgreSQL"),
                npgsql => npgsql.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName)
            )
        );

        // Repositories
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddSingleton<IOrderRepository, MongoOrderRepository>();
        services.AddScoped<IStockRepository, StockRepository>();

        // Services
        services.AddSingleton<IPasswordHasher, Argon2PasswordHasher>();
        services.AddScoped<IEventPublisher, MassTransitEventPublisher>();

        // MongoDB
        services.Configure<MongoDbSettings>(
            configuration.GetSection(MongoDbSettings.SectionName));
        services.AddSingleton<MongoDbContext>();

        // Cache
        services.Configure<CacheSettings>(
            configuration.GetSection(CacheSettings.SectionName));

        // Redis Distributed Cache
        services.Configure<RedisSettings>(
            configuration.GetSection(RedisSettings.SectionName));

        var redisSettings = configuration.GetSection(RedisSettings.SectionName).Get<RedisSettings>()!;
        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = redisSettings.ConnectionString;
            options.InstanceName = redisSettings.InstanceName;
        });
        services.AddSingleton<IConnectionMultiplexer>(_ =>
            ConnectionMultiplexer.Connect(redisSettings.ConnectionString));
        services.AddSingleton<ICacheService, RedisCacheService>();
        services.AddSingleton<ICartService, RedisCartService>();

        // RabbitMQ via MassTransit
        services.AddMassTransit(bus =>
        {
            bus.SetKebabCaseEndpointNameFormatter();

            // Allow callers (e.g., Worker) to register consumers
            configureBus?.Invoke(bus);

            bus.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host(configuration.GetConnectionString("RabbitMQ"));
                cfg.ConfigureEndpoints(context);
            });
        });

        if (includeWebAuth)
        {
            // JWT Authentication
            var jwtSettings = configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>()!;
            services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));
            services.AddScoped<IJwtService, JwtService>();

            services.AddAuthentication("Bearer")
                .AddJwtBearer("Bearer", options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = jwtSettings.Issuer,
                        ValidAudience = jwtSettings.Audience,
                        IssuerSigningKey = new SymmetricSecurityKey(
                            Encoding.UTF8.GetBytes(jwtSettings.SecretKey))
                    };
                });

            services.AddAuthorizationBuilder()
                .AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"))
                .AddPolicy("CustomerOnly", policy => policy.RequireRole("Customer"));

            // Health Checks
            var mongoConnectionString = configuration.GetConnectionString("MongoDB")!;
            var rabbitConnectionString = configuration.GetConnectionString("RabbitMQ")!;
            services.AddHealthChecks()
                .AddNpgSql(configuration.GetConnectionString("PostgreSQL")!, name: "postgresql")
                .AddMongoDb(sp => new MongoDB.Driver.MongoClient(mongoConnectionString), name: "mongodb")
                .AddRedis(configuration.GetConnectionString("Redis")!, name: "redis")
                .AddRabbitMQ(sp =>
                {
                    var factory = new RabbitMQ.Client.ConnectionFactory { Uri = new Uri(rabbitConnectionString) };
                    return factory.CreateConnectionAsync().GetAwaiter().GetResult();
                }, name: "rabbitmq");
        }

        return services;
    }
}
