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
    private readonly IOrderRepository _orderRepository;
    private readonly ILogger<CheckoutCommandHandler> _logger;

    public CheckoutCommandHandler(
        ICartService cartService,
        IOrderRepository orderRepository,
        ILogger<CheckoutCommandHandler> logger)
    {
        _cartService = cartService;
        _orderRepository = orderRepository;
        _logger = logger;
    }

    public async Task<CheckoutResponse> Handle(CheckoutCommand request, CancellationToken cancellationToken)
    {
        var cart = await _cartService.GetCartAsync(request.UserId, cancellationToken);

        if (cart.Items.Count == 0)
            throw new InvalidOperationException("Cannot checkout an empty cart");

        // Create order from cart
        var order = new Order
        {
            UserId = Guid.Parse(request.UserId),
            TotalAmount = cart.TotalAmount,
            ShippingAddress = request.ShippingAddress,
            Notes = request.Notes,
            Items = cart.Items.Select(item => new OrderItem
            {
                ProductId = item.ProductId,
                ProductName = item.ProductName,
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice
            }).ToList()
        };

        await _orderRepository.CreateAsync(order, cancellationToken);

        // Clear the cart after successful order creation
        await _cartService.ClearCartAsync(request.UserId, cancellationToken);

        _logger.LogInformation(
            "Order created from cart: {OrderId}, {ItemCount} items, total: {Total}",
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
