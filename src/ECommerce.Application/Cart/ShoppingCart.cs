namespace ECommerce.Application.Cart;

/// <summary>
/// Shopping cart stored in Redis. Not a domain entity — it's ephemeral
/// and only persists until checkout or TTL expiration.
/// </summary>
public record ShoppingCart
{
    public string UserId { get; init; } = string.Empty;
    public List<CartItem> Items { get; init; } = [];
    public DateTime UpdatedAt { get; init; } = DateTime.UtcNow;
}

public record CartItem(string ProductId, int Quantity);
