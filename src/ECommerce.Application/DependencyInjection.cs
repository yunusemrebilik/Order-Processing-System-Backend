using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace ECommerce.Application;

/// <summary>
/// Extension method to register all Application services.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = typeof(DependencyInjection).Assembly;

        // MediatR — scans this assembly for handlers
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(assembly));

        // FluentValidation — scans this assembly for validators
        services.AddValidatorsFromAssembly(assembly);

        return services;
    }
}
