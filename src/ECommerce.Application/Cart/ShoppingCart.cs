namespace ECommerce.Application.Cart;

/// <summary>
/// Shopping cart stored in Redis. Not a domain entity — it's ephemeral
/// and only persists until checkout or TTL expiration.
/// </summary>
public record ShoppingCart
{
    public string UserId { get; init; } = string.Empty;
    public List<CartItem> Items { get; init; } = [];
    public decimal TotalAmount => Items.Sum(i => i.TotalPrice);
    public DateTime UpdatedAt { get; init; } = DateTime.UtcNow;
}

public record CartItem
{
    public string ProductId { get; init; } = string.Empty;
    public string ProductName { get; init; } = string.Empty;
    public decimal UnitPrice { get; init; }
    public int Quantity { get; init; }
    public decimal TotalPrice => UnitPrice * Quantity;
}
