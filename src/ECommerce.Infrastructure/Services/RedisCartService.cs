using ECommerce.Application.Common.Settings;
using ECommerce.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using ECommerce.Application.Cart;

namespace ECommerce.Infrastructure.Services;

public class RedisCartService : ICartService
{
    private readonly IConnectionMultiplexer _redis;
    private readonly ILogger<RedisCartService> _logger;
    private readonly CacheSettings _cacheSettings;

    private const string UpdatedAtField = "_updatedAt";

    public RedisCartService(IConnectionMultiplexer redis, ILogger<RedisCartService> logger, IOptions<CacheSettings> cacheSettings)
    {
        _redis = redis;
        _logger = logger;
        _cacheSettings = cacheSettings.Value;
    }

    private static string CartKey(string userId) => $"cart:{userId}";

    public async Task<ShoppingCart> GetCartItemsAsync(string userId, CancellationToken cancellationToken = default)
    {
        var db = _redis.GetDatabase();
        var hashEntries = await db.HashGetAllAsync(CartKey(userId));

        if (hashEntries.Length == 0)
        {
            return new ShoppingCart { UserId = userId };
        }

        var items = new List<CartItem>();
        DateTime updatedAt = DateTime.UtcNow;

        foreach (var entry in hashEntries)
        {
            if (entry.Name == UpdatedAtField)
            {
                DateTime.TryParse(entry.Value, out updatedAt);
                continue;
            }

            items.Add(new CartItem(
                ProductId: entry.Name.ToString(),
                Quantity: (int)entry.Value
            ));
        }

        return new ShoppingCart
        {
            UserId = userId,
            Items = items,
            UpdatedAt = updatedAt
        };
    }

    public async Task AddOrUpdateItemAsync(string userId, string productId, int quantityChange, CancellationToken cancellationToken = default)
    {
        var db = _redis.GetDatabase();
        var key = CartKey(userId);

        var newQuantity = await db.HashIncrementAsync(key, productId, quantityChange);

        if (newQuantity <= 0)
        {
            await db.HashDeleteAsync(key, productId);
        }

        await db.HashSetAsync(key, UpdatedAtField, DateTime.UtcNow.ToString("O"), flags: CommandFlags.FireAndForget);
        await db.KeyExpireAsync(key, TimeSpan.FromDays(_cacheSettings.CartTtlInDays), CommandFlags.FireAndForget);

        _logger.LogInformation("Cart updated: User {UserId}, Product {ProductId}, New Qty: {Qty}", userId, productId, newQuantity);
    }

    public async Task RemoveItemAsync(string userId, string productId, CancellationToken cancellationToken = default)
    {
        var db = _redis.GetDatabase();
        var key = CartKey(userId);

        await db.HashDeleteAsync(key, productId);

        await db.HashSetAsync(key, UpdatedAtField, DateTime.UtcNow.ToString("O"), flags: CommandFlags.FireAndForget);
        await db.KeyExpireAsync(key, TimeSpan.FromDays(_cacheSettings.CartTtlInDays), CommandFlags.FireAndForget);
    }

    public async Task ClearCartAsync(string userId, CancellationToken cancellationToken = default)
    {
        var db = _redis.GetDatabase();
        await db.KeyDeleteAsync(CartKey(userId));
    }
}
