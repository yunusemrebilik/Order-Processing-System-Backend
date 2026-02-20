using ECommerce.Application.Common.Interfaces;
using MediatR;

namespace ECommerce.Application.Cart.AddToCart;

public class AddToCartCommandHandler : IRequestHandler<AddToCartCommand, ShoppingCart>
{
    private readonly ICartService _cartService;
    private readonly IProductRepository _productRepository;

    public AddToCartCommandHandler(ICartService cartService, IProductRepository productRepository)
    {
        _cartService = cartService;
        _productRepository = productRepository;
    }

    public async Task<ShoppingCart> Handle(AddToCartCommand request, CancellationToken cancellationToken)
    {
        // Validate product exists and is active
        var product = await _productRepository.GetByIdAsync(request.ProductId, cancellationToken);
        if (product is null || !product.IsActive)
            throw new KeyNotFoundException($"Product '{request.ProductId}' not found or inactive");

        var cartItem = new CartItem
        {
            ProductId = product.Id,
            ProductName = product.Name,
            UnitPrice = product.Price,
            Quantity = request.Quantity
        };

        return await _cartService.AddItemAsync(request.UserId, cartItem, cancellationToken);
    }
}
