using ECommerce.Application.Common.Interfaces;
using MediatR;

namespace ECommerce.Application.Cart.RemoveCartItem;

public record RemoveCartItemCommand : IRequest<ShoppingCart>
{
    public string UserId { get; init; } = string.Empty;
    public string ProductId { get; init; } = string.Empty;
}

public class RemoveCartItemCommandHandler : IRequestHandler<RemoveCartItemCommand, ShoppingCart>
{
    private readonly ICartService _cartService;

    public RemoveCartItemCommandHandler(ICartService cartService)
    {
        _cartService = cartService;
    }

    public async Task<ShoppingCart> Handle(RemoveCartItemCommand request, CancellationToken cancellationToken)
    {
        return await _cartService.RemoveItemAsync(request.UserId, request.ProductId, cancellationToken);
    }
}
