using ECommerce.Application.Common.Interfaces;
using MediatR;

namespace ECommerce.Application.Cart.GetCart;

public record GetCartQuery(string UserId) : IRequest<ShoppingCart>;

public class GetCartQueryHandler : IRequestHandler<GetCartQuery, ShoppingCart>
{
    private readonly ICartService _cartService;

    public GetCartQueryHandler(ICartService cartService)
    {
        _cartService = cartService;
    }

    public async Task<ShoppingCart> Handle(GetCartQuery request, CancellationToken cancellationToken)
    {
        return await _cartService.GetCartAsync(request.UserId, cancellationToken);
    }
}
