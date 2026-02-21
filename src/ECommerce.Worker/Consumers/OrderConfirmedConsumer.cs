using ECommerce.Application.Common.Events;
using ECommerce.Application.Common.Interfaces;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace ECommerce.Worker.Consumers;

/// <summary>
/// Reacts to OrderConfirmedEvent by clearing the user's cart.
/// Separation of concerns: stock consumer only cares about stock,
/// this consumer handles the cart cleanup.
/// </summary>
public class OrderConfirmedConsumer : IConsumer<OrderConfirmedEvent>
{
    private readonly ICartService _cartService;
    private readonly ILogger<OrderConfirmedConsumer> _logger;

    public OrderConfirmedConsumer(ICartService cartService, ILogger<OrderConfirmedConsumer> logger)
    {
        _cartService = cartService;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<OrderConfirmedEvent> context)
    {
        var message = context.Message;

        await _cartService.ClearCartAsync(message.UserId, context.CancellationToken);

        _logger.LogInformation("Cart cleared for user {UserId} after order {OrderId} confirmed",
            message.UserId, message.OrderId);
    }
}
