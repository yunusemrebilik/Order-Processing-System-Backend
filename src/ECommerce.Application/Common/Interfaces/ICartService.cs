namespace ECommerce.Application.Common.Interfaces;

/// <summary>
/// Shopping cart service — backed by Redis with TTL.
/// Cart operations are user-scoped (one cart per user).
/// </summary>
public interface ICartService
{
    Task<Cart.ShoppingCart> GetCartAsync(string userId, CancellationToken cancellationToken = default);
    Task<Cart.ShoppingCart> AddItemAsync(string userId, Cart.CartItem item, CancellationToken cancellationToken = default);
    Task<Cart.ShoppingCart> UpdateItemQuantityAsync(string userId, string productId, int quantity, CancellationToken cancellationToken = default);
    Task<Cart.ShoppingCart> RemoveItemAsync(string userId, string productId, CancellationToken cancellationToken = default);
    Task ClearCartAsync(string userId, CancellationToken cancellationToken = default);
}
