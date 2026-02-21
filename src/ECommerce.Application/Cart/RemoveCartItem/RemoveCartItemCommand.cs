using ECommerce.Application.Common.Interfaces;
using MediatR;

namespace ECommerce.Application.Cart.RemoveCartItem;

public record RemoveCartItemCommand : IRequest
{
    public string UserId { get; init; } = string.Empty;
    public string ProductId { get; init; } = string.Empty;
}

public class RemoveCartItemCommandHandler : IRequestHandler<RemoveCartItemCommand>
{
    private readonly ICartService _cartService;

    public RemoveCartItemCommandHandler(ICartService cartService)
    {
        _cartService = cartService;
    }

    public async Task Handle(RemoveCartItemCommand request, CancellationToken cancellationToken)
    {
        await _cartService.RemoveItemAsync(request.UserId, request.ProductId, cancellationToken);
    }
}
