using ECommerce.Domain.Entities;
using FluentAssertions;

namespace ECommerce.UnitTests.Domain;

public class OrderItemTests
{
    [Theory]
    [InlineData(2, 10.50, 21.00)]
    [InlineData(1, 99.99, 99.99)]
    [InlineData(5, 20.00, 100.00)]
    public void TotalPrice_ShouldReturnQuantityTimesUnitPrice(int quantity, decimal unitPrice, decimal expected)
    {
        var item = new OrderItem { Quantity = quantity, UnitPrice = unitPrice };

        item.TotalPrice.Should().Be(expected);
    }

    [Fact]
    public void TotalPrice_ShouldReturnZero_WhenQuantityIsZero()
    {
        var item = new OrderItem { Quantity = 0, UnitPrice = 50.00m };

        item.TotalPrice.Should().Be(0);
    }
}
