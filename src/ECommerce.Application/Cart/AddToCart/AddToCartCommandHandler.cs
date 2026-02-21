using ECommerce.Application.Common.Interfaces;
using MediatR;

namespace ECommerce.Application.Cart.AddToCart;

public class AddToCartCommandHandler : IRequestHandler<AddToCartCommand>
{
    private readonly ICartService _cartService;
    private readonly IProductRepository _productRepository;

    public AddToCartCommandHandler(ICartService cartService, IProductRepository productRepository)
    {
        _cartService = cartService;
        _productRepository = productRepository;
    }

    public async Task Handle(AddToCartCommand request, CancellationToken cancellationToken)
    {
        // Validate product exists and is active
        var product = await _productRepository.GetByIdAsync(request.ProductId, cancellationToken);
        if (product is null || !product.IsActive)
            throw new KeyNotFoundException($"Product '{request.ProductId}' not found or inactive");

        await _cartService.AddOrUpdateItemAsync(request.UserId, request.ProductId, request.Quantity, cancellationToken);
    }
}
