using MediatR;

namespace ECommerce.Application.Cart.AddToCart;

public record AddToCartCommand : IRequest
{
    public string UserId { get; init; } = string.Empty;
    public string ProductId { get; init; } = string.Empty;
    public int Quantity { get; init; } = 1;
}
