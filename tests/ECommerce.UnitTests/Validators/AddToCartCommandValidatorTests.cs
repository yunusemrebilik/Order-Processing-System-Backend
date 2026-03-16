using ECommerce.Application.Cart.AddToCart;
using FluentAssertions;
using FluentValidation.TestHelper;

namespace ECommerce.UnitTests.Validators;

public class AddToCartCommandValidatorTests
{
    private readonly AddToCartCommandValidator _validator = new();

    [Fact]
    public void Validate_ValidCommand_ShouldPass()
    {
        var command = new AddToCartCommand { ProductId = "507f1f77bcf86cd799439011", Quantity = 2 };

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_EmptyProductId_ShouldFail()
    {
        var command = new AddToCartCommand { ProductId = "", Quantity = 1 };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.ProductId)
              .WithErrorMessage("Product ID is required");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Validate_ZeroOrNegativeQuantity_ShouldFail(int quantity)
    {
        var command = new AddToCartCommand { ProductId = "123", Quantity = quantity };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Quantity)
              .WithErrorMessage("Quantity must be at least 1");
    }

    [Fact]
    public void Validate_QuantityExceeds100_ShouldFail()
    {
        var command = new AddToCartCommand { ProductId = "123", Quantity = 101 };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Quantity)
              .WithErrorMessage("Cannot add more than 100 of the same item");
    }
}
