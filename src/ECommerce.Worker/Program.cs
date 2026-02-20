using ECommerce.Infrastructure;
using ECommerce.Worker.Consumers;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateLogger();

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddSerilog();

// Register Infrastructure services (EF Core, repos, MassTransit, etc.)
// Skip web-specific services (JWT, Auth, Health Checks)
builder.Services.AddInfrastructure(
    builder.Configuration,
    configureBus: bus => bus.AddConsumer<OrderCreatedConsumer>(),
    includeWebAuth: false);

var host = builder.Build();
host.Run();
