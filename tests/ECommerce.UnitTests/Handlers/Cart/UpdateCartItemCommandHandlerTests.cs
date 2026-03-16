using ECommerce.Application.Cart.UpdateCartItem;
using ECommerce.Application.Common.Interfaces;
using NSubstitute;

namespace ECommerce.UnitTests.Handlers.Cart;

public class UpdateCartItemCommandHandlerTests
{
    private readonly ICartService _cartService = Substitute.For<ICartService>();
    private readonly UpdateCartItemCommandHandler _handler;

    public UpdateCartItemCommandHandlerTests()
    {
        _handler = new UpdateCartItemCommandHandler(_cartService);
    }

    [Fact]
    public async Task Handle_DelegatesToCartService()
    {
        var command = new UpdateCartItemCommand { UserId = "user-1", ProductId = "product-1", Quantity = 5 };

        await _handler.Handle(command, CancellationToken.None);

        await _cartService.Received(1).AddOrUpdateItemAsync("user-1", "product-1", 5, Arg.Any<CancellationToken>());
    }
}
