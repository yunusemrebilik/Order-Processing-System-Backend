using ECommerce.Application.Auth.Login;
using ECommerce.Application.Common.Interfaces;
using ECommerce.Domain.Entities;
using ECommerce.Domain.Enums;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace ECommerce.UnitTests.Handlers.Auth;

public class LoginCommandHandlerTests
{
    private readonly IUserRepository _userRepository = Substitute.For<IUserRepository>();
    private readonly IJwtService _jwtService = Substitute.For<IJwtService>();
    private readonly IPasswordHasher _passwordHasher = Substitute.For<IPasswordHasher>();
    private readonly ILogger<LoginCommandHandler> _logger = Substitute.For<ILogger<LoginCommandHandler>>();
    private readonly LoginCommandHandler _handler;

    public LoginCommandHandlerTests()
    {
        _handler = new LoginCommandHandler(_userRepository, _jwtService, _passwordHasher, _logger);
    }

    [Fact]
    public async Task Handle_ValidCredentials_ReturnsLoginResponse()
    {
        // Arrange
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "user@example.com",
            PasswordHash = "hashed",
            FullName = "John Doe",
            Role = UserRole.Customer
        };

        _userRepository.GetByEmailAsync("user@example.com", Arg.Any<CancellationToken>())
            .Returns(user);
        _passwordHasher.Verify("password123", "hashed").Returns(true);
        _jwtService.GenerateToken(user).Returns("jwt-token");

        var command = new LoginCommand { Email = "user@example.com", Password = "password123" };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Token.Should().Be("jwt-token");
        result.UserId.Should().Be(user.Id);
        result.Email.Should().Be("user@example.com");
        result.FullName.Should().Be("John Doe");
        result.Role.Should().Be("Customer");
    }

    [Fact]
    public async Task Handle_UserNotFound_ThrowsUnauthorizedAccessException()
    {
        _userRepository.GetByEmailAsync("unknown@example.com", Arg.Any<CancellationToken>())
            .Returns((User?)null);

        var command = new LoginCommand { Email = "unknown@example.com", Password = "any" };

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>()
                 .WithMessage("Invalid email or password");
    }

    [Fact]
    public async Task Handle_WrongPassword_ThrowsUnauthorizedAccessException()
    {
        var user = new User { Email = "user@example.com", PasswordHash = "hashed" };

        _userRepository.GetByEmailAsync("user@example.com", Arg.Any<CancellationToken>())
            .Returns(user);
        _passwordHasher.Verify("wrong", "hashed").Returns(false);

        var command = new LoginCommand { Email = "user@example.com", Password = "wrong" };

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>()
                 .WithMessage("Invalid email or password");
    }
}
