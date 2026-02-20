using ECommerce.Application.Common.Interfaces;
using MediatR;

namespace ECommerce.Application.Cart.UpdateCartItem;

public record UpdateCartItemCommand : IRequest<ShoppingCart>
{
    public string UserId { get; init; } = string.Empty;
    public string ProductId { get; init; } = string.Empty;
    public int Quantity { get; init; }
}

public class UpdateCartItemCommandHandler : IRequestHandler<UpdateCartItemCommand, ShoppingCart>
{
    private readonly ICartService _cartService;

    public UpdateCartItemCommandHandler(ICartService cartService)
    {
        _cartService = cartService;
    }

    public async Task<ShoppingCart> Handle(UpdateCartItemCommand request, CancellationToken cancellationToken)
    {
        return await _cartService.UpdateItemQuantityAsync(
            request.UserId, request.ProductId, request.Quantity, cancellationToken);
    }
}
