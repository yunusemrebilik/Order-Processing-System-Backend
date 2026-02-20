namespace ECommerce.Application.Common.Events;

/// <summary>
/// Published when a customer checks out their cart.
/// Consumed by Worker Service to validate stock and confirm the order.
/// This is a MassTransit message contract — shared between publisher (API) and consumer (Worker).
/// </summary>
public record OrderCreatedEvent
{
    public Guid OrderId { get; init; }
    public Guid UserId { get; init; }
    public List<OrderCreatedEventItem> Items { get; init; } = [];
    public decimal TotalAmount { get; init; }
    public DateTime CreatedAt { get; init; }
}

public record OrderCreatedEventItem
{
    public string ProductId { get; init; } = string.Empty;
    public string ProductName { get; init; } = string.Empty;
    public int Quantity { get; init; }
    public decimal UnitPrice { get; init; }
}
