using ECommerce.Application.Common.Interfaces;
using ECommerce.Domain.Entities;
using ECommerce.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace ECommerce.Infrastructure.Data;

public static class UserSeeder
{
    public static async Task SeedAsync(IUserRepository userRepository, IPasswordHasher passwordHasher, ILogger logger)
    {
        var adminEmail = "admin@ecommerce.com";
        
        if (await userRepository.ExistsByEmailAsync(adminEmail))
        {
            return;
        }

        var adminUser = new User
        {
            Email = adminEmail,
            FullName = "System Administrator",
            PasswordHash = passwordHasher.Hash("Password123!"),
            Role = UserRole.Admin,
            CreatedAt = DateTime.UtcNow
        };

        var createdAdmin = await userRepository.CreateAsync(adminUser);
        
        logger.LogInformation("Seeded Default Admin Account: {Email} ({UserId})", createdAdmin.Email, createdAdmin.Id);
    }
}
