using ECommerce.Application.Common.Interfaces;
using ECommerce.Application.Products.CreateProduct;
using ECommerce.Domain.Entities;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace ECommerce.UnitTests.Handlers.Products;

public class CreateProductCommandHandlerTests
{
    private readonly IProductRepository _productRepository = Substitute.For<IProductRepository>();
    private readonly ICacheService _cache = Substitute.For<ICacheService>();
    private readonly ILogger<CreateProductCommandHandler> _logger = Substitute.For<ILogger<CreateProductCommandHandler>>();
    private readonly CreateProductCommandHandler _handler;

    public CreateProductCommandHandlerTests()
    {
        _handler = new CreateProductCommandHandler(_productRepository, _cache, _logger);
    }

    [Fact]
    public async Task Handle_ValidCommand_CreatesProductAndInvalidatesCache()
    {
        // Arrange
        _productRepository.CreateAsync(Arg.Any<Product>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => callInfo.Arg<Product>());

        var command = new CreateProductCommand
        {
            Name = "Test Product",
            Description = "A test product",
            Category = "Electronics",
            Price = 99.99m,
            ImageUrl = "http://example.com/img.png",
            Attributes = new Dictionary<string, object> { { "Color", "Black" } }
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("Test Product");
        result.Category.Should().Be("Electronics");
        result.Price.Should().Be(99.99m);

        await _productRepository.Received(1).CreateAsync(
            Arg.Is<Product>(p =>
                p.Name == "Test Product" &&
                p.Description == "A test product" &&
                p.Category == "Electronics" &&
                p.Price == 99.99m),
            Arg.Any<CancellationToken>());

        await _cache.Received(1).RemoveByPrefixAsync("products:list:", Arg.Any<CancellationToken>());
    }
}
