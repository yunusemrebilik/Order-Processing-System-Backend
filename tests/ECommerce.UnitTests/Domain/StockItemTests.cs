using ECommerce.Domain.Entities;
using FluentAssertions;

namespace ECommerce.UnitTests.Domain;

public class StockItemTests
{
    [Fact]
    public void AvailableQuantity_ShouldReturnQuantityMinusReserved()
    {
        var stock = new StockItem { Quantity = 100, ReservedQuantity = 30 };

        stock.AvailableQuantity.Should().Be(70);
    }

    [Fact]
    public void AvailableQuantity_ShouldReturnZero_WhenFullyReserved()
    {
        var stock = new StockItem { Quantity = 50, ReservedQuantity = 50 };

        stock.AvailableQuantity.Should().Be(0);
    }

    [Fact]
    public void AvailableQuantity_ReturnsNegative_WhenOverReserved_DocumentsCurrentBehavior()
    {
        // NOTE: The domain entity does not guard against over-reservation.
        // This test documents the current behavior. If a business rule is
        // needed to prevent this, it should be enforced at the service/repository layer.
        var stock = new StockItem { Quantity = 10, ReservedQuantity = 15 };

        stock.AvailableQuantity.Should().Be(-5);
    }
}
