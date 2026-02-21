namespace ECommerce.Application.Common.Events;

/// <summary>
/// Published by OrderCreatedConsumer after stock is validated and order is confirmed.
/// Consumed by OrderConfirmedConsumer to clear the user's cart.
/// </summary>
public record OrderConfirmedEvent
{
    public string OrderId { get; init; } = string.Empty;
    public string UserId { get; init; } = string.Empty;
}
