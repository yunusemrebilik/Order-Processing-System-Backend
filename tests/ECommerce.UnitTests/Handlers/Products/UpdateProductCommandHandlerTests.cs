using ECommerce.Application.Common.Interfaces;
using ECommerce.Application.Products.UpdateProduct;
using ECommerce.Domain.Entities;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace ECommerce.UnitTests.Handlers.Products;

public class UpdateProductCommandHandlerTests
{
    private readonly IProductRepository _productRepository = Substitute.For<IProductRepository>();
    private readonly ICacheService _cache = Substitute.For<ICacheService>();
    private readonly ILogger<UpdateProductCommandHandler> _logger = Substitute.For<ILogger<UpdateProductCommandHandler>>();
    private readonly UpdateProductCommandHandler _handler;

    public UpdateProductCommandHandlerTests()
    {
        _handler = new UpdateProductCommandHandler(_productRepository, _cache, _logger);
    }

    [Fact]
    public async Task Handle_ExistingProduct_UpdatesAndInvalidatesCache()
    {
        // Arrange
        var productId = "507f1f77bcf86cd799439011";
        var existingProduct = new Product { Id = productId, Name = "Old Name", Price = 50m };

        _productRepository.GetByIdAsync(productId, Arg.Any<CancellationToken>())
            .Returns(existingProduct);
        _productRepository.UpdateAsync(Arg.Any<Product>(), Arg.Any<CancellationToken>())
            .Returns(true);

        var command = new UpdateProductCommand
        {
            Id = productId,
            Name = "New Name",
            Description = "Updated description",
            Category = "Updated Category",
            Price = 149.99m,
            IsActive = true
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeTrue();

        await _cache.Received(1).RemoveAsync($"products:info:{productId}", Arg.Any<CancellationToken>());
        await _cache.Received(1).RemoveAsync($"products:price:{productId}", Arg.Any<CancellationToken>());
        await _cache.Received(1).RemoveByPrefixAsync("products:list:", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_NonExistentProduct_ThrowsKeyNotFoundException()
    {
        _productRepository.GetByIdAsync("nonexistent", Arg.Any<CancellationToken>())
            .Returns((Product?)null);

        var command = new UpdateProductCommand { Id = "nonexistent", Name = "X", Category = "C", Price = 1m };

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>()
                 .WithMessage("*nonexistent*not found*");
    }
}
