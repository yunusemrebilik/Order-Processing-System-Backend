using ECommerce.Application.Products.UpdateProductPrice;
using FluentAssertions;
using FluentValidation.TestHelper;

namespace ECommerce.UnitTests.Validators;

public class UpdateProductPriceCommandValidatorTests
{
    private readonly UpdateProductPriceCommandValidator _validator = new();

    [Fact]
    public void Validate_ValidCommand_ShouldPass()
    {
        var command = new UpdateProductPriceCommand { Id = "507f1f77bcf86cd799439011", Price = 49.99m };

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_EmptyId_ShouldFail()
    {
        var command = new UpdateProductPriceCommand { Id = "", Price = 10m };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Id)
              .WithErrorMessage("Product ID is required.");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public void Validate_ZeroOrNegativePrice_ShouldFail(decimal price)
    {
        var command = new UpdateProductPriceCommand { Id = "123", Price = price };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Price)
              .WithErrorMessage("Price must be greater than zero.");
    }
}
