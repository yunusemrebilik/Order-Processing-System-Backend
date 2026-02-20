using System.Text.Json;
using ECommerce.Application.Cart;
using ECommerce.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace ECommerce.Infrastructure.Services;

/// <summary>
/// Redis-backed shopping cart service.
/// Each user's cart is stored as a JSON string with a 7-day TTL.
/// Key format: "cart:{userId}"
/// </summary>
public class RedisCartService : ICartService
{
    private readonly IConnectionMultiplexer _redis;
    private readonly ILogger<RedisCartService> _logger;
    private static readonly TimeSpan CartTtl = TimeSpan.FromDays(7);

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    public RedisCartService(IConnectionMultiplexer redis, ILogger<RedisCartService> logger)
    {
        _redis = redis;
        _logger = logger;
    }

    private static string CartKey(string userId) => $"cart:{userId}";

    public async Task<ShoppingCart> GetCartAsync(string userId, CancellationToken cancellationToken = default)
    {
        var db = _redis.GetDatabase();
        var data = await db.StringGetAsync(CartKey(userId));

        if (data.IsNullOrEmpty)
            return new ShoppingCart { UserId = userId };

        return JsonSerializer.Deserialize<ShoppingCart>(data!, JsonOptions)
               ?? new ShoppingCart { UserId = userId };
    }

    public async Task<ShoppingCart> AddItemAsync(string userId, CartItem item, CancellationToken cancellationToken = default)
    {
        var cart = await GetCartAsync(userId, cancellationToken);
        var items = cart.Items.ToList();

        // If product already in cart, increment quantity
        var existing = items.FindIndex(i => i.ProductId == item.ProductId);
        if (existing >= 0)
        {
            items[existing] = items[existing] with
            {
                Quantity = items[existing].Quantity + item.Quantity
            };
        }
        else
        {
            items.Add(item);
        }

        var updatedCart = cart with { Items = items, UpdatedAt = DateTime.UtcNow };
        await SaveCartAsync(userId, updatedCart);

        _logger.LogInformation("Cart item added: {Product} x{Qty} for user {UserId}",
            item.ProductName, item.Quantity, userId);

        return updatedCart;
    }

    public async Task<ShoppingCart> UpdateItemQuantityAsync(string userId, string productId, int quantity, CancellationToken cancellationToken = default)
    {
        var cart = await GetCartAsync(userId, cancellationToken);
        var items = cart.Items.ToList();

        var index = items.FindIndex(i => i.ProductId == productId);
        if (index < 0)
            throw new KeyNotFoundException($"Product '{productId}' not found in cart");

        if (quantity <= 0)
        {
            items.RemoveAt(index);
        }
        else
        {
            items[index] = items[index] with { Quantity = quantity };
        }

        var updatedCart = cart with { Items = items, UpdatedAt = DateTime.UtcNow };
        await SaveCartAsync(userId, updatedCart);

        return updatedCart;
    }

    public async Task<ShoppingCart> RemoveItemAsync(string userId, string productId, CancellationToken cancellationToken = default)
    {
        var cart = await GetCartAsync(userId, cancellationToken);
        var items = cart.Items.Where(i => i.ProductId != productId).ToList();

        var updatedCart = cart with { Items = items, UpdatedAt = DateTime.UtcNow };
        await SaveCartAsync(userId, updatedCart);

        _logger.LogInformation("Cart item removed: {ProductId} for user {UserId}", productId, userId);

        return updatedCart;
    }

    public async Task ClearCartAsync(string userId, CancellationToken cancellationToken = default)
    {
        var db = _redis.GetDatabase();
        await db.KeyDeleteAsync(CartKey(userId));
        _logger.LogInformation("Cart cleared for user {UserId}", userId);
    }

    private async Task SaveCartAsync(string userId, ShoppingCart cart)
    {
        var db = _redis.GetDatabase();
        var json = JsonSerializer.Serialize(cart, JsonOptions);
        await db.StringSetAsync(CartKey(userId), json, CartTtl);
    }
}
