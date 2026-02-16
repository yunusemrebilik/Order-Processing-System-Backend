namespace ECommerce.Domain.Events;

/// <summary>
/// Published when a new order is created.
/// Consumed by the Worker to process stock deduction and status updates.
/// </summary>
public record OrderCreatedEvent
{
    public Guid OrderId { get; init; }
    public Guid UserId { get; init; }
    public List<OrderItemEvent> Items { get; init; } = [];
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
}

public record OrderItemEvent
{
    public string ProductId { get; init; } = string.Empty;
    public string ProductName { get; init; } = string.Empty;
    public int Quantity { get; init; }
    public decimal UnitPrice { get; init; }
}
