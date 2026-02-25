using ECommerce.Api.Middleware;
using ECommerce.Application;
using ECommerce.Infrastructure;
using ECommerce.Api.Settings;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// ── Serilog ──────────────────────────────────────────────────────────────
builder.Host.UseSerilog((context, loggerConfig) =>
    loggerConfig.ReadFrom.Configuration(context.Configuration));

// ── Services ─────────────────────────────────────────────────────────────
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddControllers();

// ── Rate Limiting ─────────────────────────────────────────────────────────
builder.Services.Configure<RateLimitSettings>(builder.Configuration.GetSection(RateLimitSettings.SectionName));

// ── Swagger ──────────────────────────────────────────────────────────────
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new()
    {
        Title = "ECommerce Order Processing API",
        Version = "v1",
        Description = "A showcase ASP.NET Core backend demonstrating PostgreSQL, MongoDB, Redis, and RabbitMQ integration."
    });

    // JWT Bearer auth in Swagger UI
    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Enter your JWT token"
    });

    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// ── Middleware Pipeline ──────────────────────────────────────────────────
app.UseMiddleware<CorrelationIdMiddleware>();
app.UseMiddleware<GlobalExceptionHandlerMiddleware>();
app.UseSerilogRequestLogging();
app.UseMiddleware<RedisRateLimitingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");

// ── Seed Data (Development only) ────────────────────────────────────────
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var productRepo = scope.ServiceProvider.GetRequiredService<ECommerce.Application.Common.Interfaces.IProductRepository>();
    await ECommerce.Infrastructure.Data.MongoDbSeeder.SeedAsync(productRepo);

    var stockRepo = scope.ServiceProvider.GetRequiredService<ECommerce.Application.Common.Interfaces.IStockRepository>();
    await ECommerce.Infrastructure.Data.StockSeeder.SeedAsync(productRepo, stockRepo);
    
    var userRepo = scope.ServiceProvider.GetRequiredService<ECommerce.Application.Common.Interfaces.IUserRepository>();
    var passwordHasher = scope.ServiceProvider.GetRequiredService<ECommerce.Application.Common.Interfaces.IPasswordHasher>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    await ECommerce.Infrastructure.Data.UserSeeder.SeedAsync(userRepo, passwordHasher, logger);
}

app.Run();
