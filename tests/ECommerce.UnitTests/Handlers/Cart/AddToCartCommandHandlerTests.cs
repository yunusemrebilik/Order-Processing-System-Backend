using ECommerce.Application.Cart.AddToCart;
using ECommerce.Application.Common.Interfaces;
using ECommerce.Domain.Entities;
using FluentAssertions;
using NSubstitute;

namespace ECommerce.UnitTests.Handlers.Cart;

public class AddToCartCommandHandlerTests
{
    private readonly ICartService _cartService = Substitute.For<ICartService>();
    private readonly IProductRepository _productRepository = Substitute.For<IProductRepository>();
    private readonly AddToCartCommandHandler _handler;

    public AddToCartCommandHandlerTests()
    {
        _handler = new AddToCartCommandHandler(_cartService, _productRepository);
    }

    [Fact]
    public async Task Handle_ActiveProduct_AddsToCart()
    {
        // Arrange
        var productId = "507f1f77bcf86cd799439011";
        var product = new Product { Id = productId, Name = "Phone", IsActive = true };

        _productRepository.GetByIdAsync(productId, Arg.Any<CancellationToken>())
            .Returns(product);

        var command = new AddToCartCommand { UserId = "user-1", ProductId = productId, Quantity = 2 };

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        await _cartService.Received(1).AddOrUpdateItemAsync("user-1", productId, 2, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_InactiveProduct_ThrowsKeyNotFoundException()
    {
        var productId = "507f1f77bcf86cd799439011";
        var product = new Product { Id = productId, Name = "Discontinued", IsActive = false };

        _productRepository.GetByIdAsync(productId, Arg.Any<CancellationToken>())
            .Returns(product);

        var command = new AddToCartCommand { UserId = "user-1", ProductId = productId, Quantity = 1 };

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>()
                 .WithMessage("*not found or inactive*");
    }

    [Fact]
    public async Task Handle_NonExistentProduct_ThrowsKeyNotFoundException()
    {
        _productRepository.GetByIdAsync("missing", Arg.Any<CancellationToken>())
            .Returns((Product?)null);

        var command = new AddToCartCommand { UserId = "user-1", ProductId = "missing", Quantity = 1 };

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>()
                 .WithMessage("*not found or inactive*");
    }
}
