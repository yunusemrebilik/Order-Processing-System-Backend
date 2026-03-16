using ECommerce.Application.Auth.Login;
using FluentAssertions;
using FluentValidation.TestHelper;

namespace ECommerce.UnitTests.Validators;

public class LoginCommandValidatorTests
{
    private readonly LoginCommandValidator _validator = new();

    [Fact]
    public void Validate_ValidCommand_ShouldPass()
    {
        var command = new LoginCommand { Email = "user@example.com", Password = "password123" };

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_EmptyEmail_ShouldFail()
    {
        var command = new LoginCommand { Email = "", Password = "password123" };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Email)
              .WithErrorMessage("Email is required");
    }

    [Fact]
    public void Validate_InvalidEmailFormat_ShouldFail()
    {
        var command = new LoginCommand { Email = "not-an-email", Password = "password123" };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Email)
              .WithErrorMessage("Invalid email format");
    }

    [Fact]
    public void Validate_EmptyPassword_ShouldFail()
    {
        var command = new LoginCommand { Email = "user@example.com", Password = "" };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Password)
              .WithErrorMessage("Password is required");
    }
}
