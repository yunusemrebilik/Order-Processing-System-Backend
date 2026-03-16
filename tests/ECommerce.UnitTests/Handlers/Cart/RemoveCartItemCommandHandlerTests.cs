using ECommerce.Application.Cart.RemoveCartItem;
using ECommerce.Application.Common.Interfaces;
using NSubstitute;

namespace ECommerce.UnitTests.Handlers.Cart;

public class RemoveCartItemCommandHandlerTests
{
    private readonly ICartService _cartService = Substitute.For<ICartService>();
    private readonly RemoveCartItemCommandHandler _handler;

    public RemoveCartItemCommandHandlerTests()
    {
        _handler = new RemoveCartItemCommandHandler(_cartService);
    }

    [Fact]
    public async Task Handle_DelegatesToCartService()
    {
        var command = new RemoveCartItemCommand { UserId = "user-1", ProductId = "product-1" };

        await _handler.Handle(command, CancellationToken.None);

        await _cartService.Received(1).RemoveItemAsync("user-1", "product-1", Arg.Any<CancellationToken>());
    }
}
