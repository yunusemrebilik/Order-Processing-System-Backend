namespace ECommerce.Domain.Events;

/// <summary>
/// Published when order status changes (confirmed, shipped, etc.).
/// Can be used for notifications or analytics.
/// </summary>
public record OrderStatusChangedEvent
{
    public Guid OrderId { get; init; }
    public string PreviousStatus { get; init; } = string.Empty;
    public string NewStatus { get; init; } = string.Empty;
    public DateTime ChangedAt { get; init; } = DateTime.UtcNow;
}
