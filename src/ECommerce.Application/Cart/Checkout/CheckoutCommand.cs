using ECommerce.Application.Common.Events;
using ECommerce.Application.Common.Interfaces;
using ECommerce.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ECommerce.Application.Cart.Checkout;

public record CheckoutCommand : IRequest<CheckoutResponse>
{
    public string UserId { get; init; } = string.Empty;
    public string? ShippingAddress { get; init; }
    public string? Notes { get; init; }
}

public record CheckoutResponse
{
    public Guid OrderId { get; init; }
    public decimal TotalAmount { get; init; }
    public int ItemCount { get; init; }
    public string Status { get; init; } = string.Empty;
}

public class CheckoutCommandHandler : IRequestHandler<CheckoutCommand, CheckoutResponse>
{
    private readonly ICartService _cartService;
    private readonly IProductRepository _productRepository;
    private readonly IOrderRepository _orderRepository;
    private readonly IEventPublisher _eventPublisher;
    private readonly ILogger<CheckoutCommandHandler> _logger;

    public CheckoutCommandHandler(
        ICartService cartService,
        IProductRepository productRepository,
        IOrderRepository orderRepository,
        IEventPublisher eventPublisher,
        ILogger<CheckoutCommandHandler> logger)
    {
        _cartService = cartService;
        _productRepository = productRepository;
        _orderRepository = orderRepository;
        _eventPublisher = eventPublisher;
        _logger = logger;
    }

    public async Task<CheckoutResponse> Handle(CheckoutCommand request, CancellationToken cancellationToken)
    {
        // 1. Get cart items (productId → quantity) from Redis
        var cartItems = await _cartService.GetCartItemsAsync(request.UserId, cancellationToken);

        if (cartItems.Items.Any() is false)
            throw new InvalidOperationException("Cannot checkout an empty cart");

        // 2. Batch-fetch current product data from MongoDB (single $in query)
        //    This ensures we use the CURRENT price, not a stale cached one.
        var productIds = cartItems.Items.Select(i => i.ProductId).ToList();
        var products = await _productRepository.GetByIdsAsync(productIds, cancellationToken);
        var productMap = products.ToDictionary(p => p.Id);

        // 3. Validate all products and build order items
        var orderItems = new List<OrderItem>();
        foreach (var (productId, quantity) in cartItems.Items)
        {
            if (!productMap.TryGetValue(productId, out var product) || !product.IsActive)
                throw new InvalidOperationException($"Product '{productId}' is no longer available");

            orderItems.Add(new OrderItem
            {
                ProductId = product.Id,
                ProductName = product.Name,
                Quantity = quantity,
                UnitPrice = product.Price
            });
        }

        // 3. Create order with fresh prices
        var totalAmount = orderItems.Sum(i => i.UnitPrice * i.Quantity);

        var order = new Order
        {
            UserId = Guid.Parse(request.UserId),
            TotalAmount = totalAmount,
            ShippingAddress = request.ShippingAddress,
            Notes = request.Notes,
            Items = orderItems
        };

        await _orderRepository.CreateAsync(order, cancellationToken);

        // 4. Publish OrderCreated event for async processing (stock validation)
        // Cart is NOT cleared here — it stays intact until the Worker confirms stock
        // and publishes OrderConfirmedEvent, which triggers cart clearing.
        await _eventPublisher.PublishAsync(new OrderCreatedEvent
        {
            OrderId = order.Id,
            UserId = order.UserId,
            TotalAmount = order.TotalAmount,
            CreatedAt = order.CreatedAt,
            Items = order.Items.Select(i => new OrderCreatedEventItem
            {
                ProductId = i.ProductId,
                ProductName = i.ProductName,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice
            }).ToList()
        }, cancellationToken);

        _logger.LogInformation(
            "Order created and event published: {OrderId}, {ItemCount} items, total: {Total}",
            order.Id, order.Items.Count, order.TotalAmount);

        return new CheckoutResponse
        {
            OrderId = order.Id,
            TotalAmount = order.TotalAmount,
            ItemCount = order.Items.Count,
            Status = order.Status.ToString()
        };
    }
}
