using ECommerce.Application.Cart;
using ECommerce.Application.Cart.GetCart;
using ECommerce.Application.Common.Interfaces;
using FluentAssertions;
using NSubstitute;

namespace ECommerce.UnitTests.Handlers.Cart;

public class GetCartQueryHandlerTests
{
    private readonly ICartService _cartService = Substitute.For<ICartService>();
    private readonly GetCartQueryHandler _handler;

    public GetCartQueryHandlerTests()
    {
        _handler = new GetCartQueryHandler(_cartService);
    }

    [Fact]
    public async Task Handle_ReturnsCartFromService()
    {
        // Arrange
        var expectedCart = new ShoppingCart
        {
            UserId = "user-1",
            Items = [new CartItem("product-1", 3)]
        };

        _cartService.GetCartItemsAsync("user-1", Arg.Any<CancellationToken>())
            .Returns(expectedCart);

        // Act
        var result = await _handler.Handle(new GetCartQuery("user-1"), CancellationToken.None);

        // Assert
        result.Should().BeSameAs(expectedCart);
    }
}
