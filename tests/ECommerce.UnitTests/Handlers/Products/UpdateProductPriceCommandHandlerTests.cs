using ECommerce.Application.Common.Interfaces;
using ECommerce.Application.Products.UpdateProductPrice;
using ECommerce.Domain.Entities;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace ECommerce.UnitTests.Handlers.Products;

public class UpdateProductPriceCommandHandlerTests
{
    private readonly IProductRepository _productRepository = Substitute.For<IProductRepository>();
    private readonly ICacheService _cache = Substitute.For<ICacheService>();
    private readonly ILogger<UpdateProductPriceCommandHandler> _logger = Substitute.For<ILogger<UpdateProductPriceCommandHandler>>();
    private readonly UpdateProductPriceCommandHandler _handler;

    public UpdateProductPriceCommandHandlerTests()
    {
        _handler = new UpdateProductPriceCommandHandler(_productRepository, _cache, _logger);
    }

    [Fact]
    public async Task Handle_ExistingProduct_UpdatesPriceAndInvalidatesPriceCache()
    {
        // Arrange
        var productId = "507f1f77bcf86cd799439011";
        var product = new Product { Id = productId, Name = "Phone", Price = 100m };

        _productRepository.GetByIdAsync(productId, Arg.Any<CancellationToken>())
            .Returns(product);
        _productRepository.UpdateAsync(Arg.Any<Product>(), Arg.Any<CancellationToken>())
            .Returns(true);

        var command = new UpdateProductPriceCommand { Id = productId, Price = 79.99m };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeTrue();

        // Should only invalidate price cache, NOT info cache
        await _cache.Received(1).RemoveAsync($"products:price:{productId}", Arg.Any<CancellationToken>());
        await _cache.DidNotReceive().RemoveAsync($"products:info:{productId}", Arg.Any<CancellationToken>());
        await _cache.Received(1).RemoveByPrefixAsync("products:list:", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_NonExistentProduct_ThrowsKeyNotFoundException()
    {
        _productRepository.GetByIdAsync("nonexistent", Arg.Any<CancellationToken>())
            .Returns((Product?)null);

        var command = new UpdateProductPriceCommand { Id = "nonexistent", Price = 10m };

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>()
                 .WithMessage("*nonexistent*not found*");
    }
}
