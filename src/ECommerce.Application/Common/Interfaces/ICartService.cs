using ECommerce.Application.Cart;

namespace ECommerce.Application.Common.Interfaces;

/// <summary>
/// Shopping cart service — backed by Redis with TTL.
/// Cart operations are user-scoped (one cart per user).
/// </summary>
public interface ICartService
{
    Task<ShoppingCart> GetCartItemsAsync(string userId, CancellationToken cancellationToken = default);
    Task AddOrUpdateItemAsync(string userId, string productId, int quantityChange, CancellationToken cancellationToken = default);
    Task RemoveItemAsync(string userId, string productId, CancellationToken cancellationToken = default);
    Task ClearCartAsync(string userId, CancellationToken cancellationToken = default);
}
