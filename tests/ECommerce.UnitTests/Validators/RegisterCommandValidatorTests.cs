using ECommerce.Application.Auth.Register;
using FluentAssertions;
using FluentValidation.TestHelper;

namespace ECommerce.UnitTests.Validators;

public class RegisterCommandValidatorTests
{
    private readonly RegisterCommandValidator _validator = new();

    [Fact]
    public void Validate_ValidCommand_ShouldPass()
    {
        var command = new RegisterCommand
        {
            Email = "user@example.com",
            Password = "password123",
            FullName = "John Doe"
        };

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_EmptyEmail_ShouldFail()
    {
        var command = new RegisterCommand { Email = "", Password = "password123", FullName = "John" };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void Validate_InvalidEmailFormat_ShouldFail()
    {
        var command = new RegisterCommand { Email = "invalid", Password = "password123", FullName = "John" };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Email)
              .WithErrorMessage("Invalid email format");
    }

    [Fact]
    public void Validate_EmptyPassword_ShouldFail()
    {
        var command = new RegisterCommand { Email = "user@example.com", Password = "", FullName = "John" };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public void Validate_ShortPassword_ShouldFail()
    {
        var command = new RegisterCommand { Email = "user@example.com", Password = "12345", FullName = "John" };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Password)
              .WithErrorMessage("Password must be at least 6 characters");
    }

    [Fact]
    public void Validate_EmptyFullName_ShouldFail()
    {
        var command = new RegisterCommand { Email = "user@example.com", Password = "password123", FullName = "" };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.FullName);
    }

    [Fact]
    public void Validate_FullNameExceeds200Characters_ShouldFail()
    {
        var command = new RegisterCommand
        {
            Email = "user@example.com",
            Password = "password123",
            FullName = new string('A', 201)
        };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.FullName)
              .WithErrorMessage("Full name must not exceed 200 characters");
    }
}
