using ECommerce.Application.Products.CreateProduct;
using FluentAssertions;
using FluentValidation.TestHelper;

namespace ECommerce.UnitTests.Validators;

public class CreateProductCommandValidatorTests
{
    private readonly CreateProductCommandValidator _validator = new();

    [Fact]
    public void Validate_ValidCommand_ShouldPass()
    {
        var command = new CreateProductCommand
        {
            Name = "Test Product",
            Category = "Electronics",
            Price = 99.99m
        };

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_EmptyName_ShouldFail()
    {
        var command = new CreateProductCommand { Name = "", Category = "Electronics", Price = 10m };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Name)
              .WithErrorMessage("Product name is required");
    }

    [Fact]
    public void Validate_NameExceeds300Characters_ShouldFail()
    {
        var command = new CreateProductCommand
        {
            Name = new string('A', 301),
            Category = "Electronics",
            Price = 10m
        };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Name)
              .WithErrorMessage("Product name must not exceed 300 characters");
    }

    [Fact]
    public void Validate_EmptyCategory_ShouldFail()
    {
        var command = new CreateProductCommand { Name = "Product", Category = "", Price = 10m };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Category)
              .WithErrorMessage("Category is required");
    }

    [Fact]
    public void Validate_CategoryExceeds100Characters_ShouldFail()
    {
        var command = new CreateProductCommand
        {
            Name = "Product",
            Category = new string('C', 101),
            Price = 10m
        };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Category)
              .WithErrorMessage("Category must not exceed 100 characters");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public void Validate_ZeroOrNegativePrice_ShouldFail(decimal price)
    {
        var command = new CreateProductCommand { Name = "Product", Category = "Cat", Price = price };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Price)
              .WithErrorMessage("Price must be greater than zero");
    }
}
