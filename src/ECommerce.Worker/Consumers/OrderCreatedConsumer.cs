using ECommerce.Application.Common.Events;
using ECommerce.Application.Common.Interfaces;
using ECommerce.Domain.Enums;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace ECommerce.Worker.Consumers;

/// <summary>
/// Consumes OrderCreated events from RabbitMQ.
/// Validates stock availability, reserves stock, deducts it, and confirms the order.
/// If stock is insufficient, the order is rejected.
/// </summary>
public class OrderCreatedConsumer : IConsumer<OrderCreatedEvent>
{
    private readonly IStockRepository _stockRepository;
    private readonly IOrderRepository _orderRepository;
    private readonly ILogger<OrderCreatedConsumer> _logger;

    public OrderCreatedConsumer(
        IStockRepository stockRepository,
        IOrderRepository orderRepository,
        ILogger<OrderCreatedConsumer> logger)
    {
        _stockRepository = stockRepository;
        _orderRepository = orderRepository;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<OrderCreatedEvent> context)
    {
        var order = context.Message;
        _logger.LogInformation("Processing order {OrderId} with {ItemCount} items",
            order.OrderId, order.Items.Count);

        try
        {
            // Step 1: Validate stock availability for all items
            var productIds = order.Items.Select(i => i.ProductId).ToList();
            var stockItems = await _stockRepository.GetByProductIdsAsync(productIds);

            var insufficientItems = new List<string>();
            foreach (var item in order.Items)
            {
                var stock = stockItems.FirstOrDefault(s => s.ProductId == item.ProductId);
                if (stock is null || stock.AvailableQuantity < item.Quantity)
                {
                    insufficientItems.Add(
                        $"{item.ProductName} (requested: {item.Quantity}, available: {stock?.AvailableQuantity ?? 0})");
                }
            }

            if (insufficientItems.Count > 0)
            {
                _logger.LogWarning("Order {OrderId} rejected — insufficient stock: {Items}",
                    order.OrderId, string.Join(", ", insufficientItems));

                await _orderRepository.UpdateStatusAsync(order.OrderId, OrderStatus.Rejected);
                return;
            }

            // Step 2: Reserve stock
            foreach (var item in order.Items)
            {
                await _stockRepository.ReserveStockAsync(item.ProductId, item.Quantity);
            }

            _logger.LogInformation("Stock reserved for order {OrderId}", order.OrderId);

            // Step 3: Deduct stock (finalize)
            foreach (var item in order.Items)
            {
                await _stockRepository.DeductStockAsync(item.ProductId, item.Quantity);
            }

            // Step 4: Confirm the order
            await _orderRepository.UpdateStatusAsync(order.OrderId, OrderStatus.Confirmed);

            // Step 5: Publish confirmation event (triggers cart clearing in separate consumer)
            await context.Publish(new OrderConfirmedEvent
            {
                OrderId = order.OrderId.ToString(),
                UserId = order.UserId.ToString()
            });

            _logger.LogInformation(
                "Order {OrderId} confirmed — stock deducted for {ItemCount} items, total: {Total}",
                order.OrderId, order.Items.Count, order.TotalAmount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process order {OrderId}", order.OrderId);

            // Release any reservations that were made
            foreach (var item in order.Items)
            {
                try
                {
                    await _stockRepository.ReleaseReservationAsync(item.ProductId, item.Quantity);
                }
                catch
                {
                    // Best-effort release
                }
            }

            await _orderRepository.UpdateStatusAsync(order.OrderId, OrderStatus.Rejected);

            throw; // Re-throw so MassTransit retries / moves to error queue
        }
    }
}
