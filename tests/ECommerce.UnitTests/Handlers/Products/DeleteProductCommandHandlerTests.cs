using ECommerce.Application.Common.Interfaces;
using ECommerce.Application.Products.DeleteProduct;
using ECommerce.Domain.Entities;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace ECommerce.UnitTests.Handlers.Products;

public class DeleteProductCommandHandlerTests
{
    private readonly IProductRepository _productRepository = Substitute.For<IProductRepository>();
    private readonly ICacheService _cache = Substitute.For<ICacheService>();
    private readonly ILogger<DeleteProductCommandHandler> _logger = Substitute.For<ILogger<DeleteProductCommandHandler>>();
    private readonly DeleteProductCommandHandler _handler;

    public DeleteProductCommandHandlerTests()
    {
        _handler = new DeleteProductCommandHandler(_productRepository, _cache, _logger);
    }

    [Fact]
    public async Task Handle_ExistingProduct_DeletesAndInvalidatesCache()
    {
        // Arrange
        var productId = "507f1f77bcf86cd799439011";
        var product = new Product { Id = productId, Name = "Test" };

        _productRepository.GetByIdAsync(productId, Arg.Any<CancellationToken>())
            .Returns(product);
        _productRepository.DeleteAsync(productId, Arg.Any<CancellationToken>())
            .Returns(true);

        // Act
        var result = await _handler.Handle(new DeleteProductCommand(productId), CancellationToken.None);

        // Assert
        result.Should().BeTrue();

        await _cache.Received(1).RemoveAsync($"products:info:{productId}", Arg.Any<CancellationToken>());
        await _cache.Received(1).RemoveAsync($"products:price:{productId}", Arg.Any<CancellationToken>());
        await _cache.Received(1).RemoveByPrefixAsync("products:list:", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_NonExistentProduct_ThrowsKeyNotFoundException()
    {
        var productId = "nonexistent";

        _productRepository.GetByIdAsync(productId, Arg.Any<CancellationToken>())
            .Returns((Product?)null);

        var act = () => _handler.Handle(new DeleteProductCommand(productId), CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>()
                 .WithMessage($"*{productId}*not found*");
    }
}
