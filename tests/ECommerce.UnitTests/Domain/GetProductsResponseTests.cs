using ECommerce.Application.Products.GetProducts;
using FluentAssertions;

namespace ECommerce.UnitTests.Domain;

public class GetProductsResponseTests
{
    [Theory]
    [InlineData(100, 10, 10)]
    [InlineData(101, 10, 11)]
    [InlineData(0, 10, 0)]
    [InlineData(1, 10, 1)]
    [InlineData(10, 10, 1)]
    [InlineData(25, 5, 5)]
    public void TotalPages_ShouldCeilCorrectly(long totalCount, int pageSize, int expectedPages)
    {
        var response = new GetProductsResponse
        {
            TotalCount = totalCount,
            PageSize = pageSize
        };

        response.TotalPages.Should().Be(expectedPages);
    }
}
