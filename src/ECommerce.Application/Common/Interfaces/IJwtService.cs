using ECommerce.Domain.Entities;

namespace ECommerce.Application.Common.Interfaces;

/// <summary>
/// JWT token generation service interface.
/// Defined in Application, implemented in Infrastructure.
/// </summary>
public interface IJwtService
{
    string GenerateToken(User user);
}
