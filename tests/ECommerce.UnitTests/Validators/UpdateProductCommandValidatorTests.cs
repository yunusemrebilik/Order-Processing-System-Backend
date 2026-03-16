using ECommerce.Application.Products.UpdateProduct;
using FluentAssertions;
using FluentValidation.TestHelper;

namespace ECommerce.UnitTests.Validators;

public class UpdateProductCommandValidatorTests
{
    private readonly UpdateProductCommandValidator _validator = new();

    [Fact]
    public void Validate_ValidCommand_ShouldPass()
    {
        var command = new UpdateProductCommand
        {
            Id = "507f1f77bcf86cd799439011",
            Name = "Updated Product",
            Category = "Electronics",
            Price = 149.99m
        };

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_EmptyId_ShouldFail()
    {
        var command = new UpdateProductCommand { Id = "", Name = "P", Category = "C", Price = 10m };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Id)
              .WithErrorMessage("Product ID is required");
    }

    [Fact]
    public void Validate_EmptyName_ShouldFail()
    {
        var command = new UpdateProductCommand { Id = "123", Name = "", Category = "C", Price = 10m };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Validate_EmptyCategory_ShouldFail()
    {
        var command = new UpdateProductCommand { Id = "123", Name = "P", Category = "", Price = 10m };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Category);
    }

    [Fact]
    public void Validate_ZeroPrice_ShouldFail()
    {
        var command = new UpdateProductCommand { Id = "123", Name = "P", Category = "C", Price = 0m };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Price);
    }
}
