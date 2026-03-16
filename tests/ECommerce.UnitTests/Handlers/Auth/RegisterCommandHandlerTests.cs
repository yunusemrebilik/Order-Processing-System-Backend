using ECommerce.Application.Auth.Register;
using ECommerce.Application.Common.Interfaces;
using ECommerce.Domain.Entities;
using ECommerce.Domain.Enums;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace ECommerce.UnitTests.Handlers.Auth;

public class RegisterCommandHandlerTests
{
    private readonly IUserRepository _userRepository = Substitute.For<IUserRepository>();
    private readonly IPasswordHasher _passwordHasher = Substitute.For<IPasswordHasher>();
    private readonly ILogger<RegisterCommandHandler> _logger = Substitute.For<ILogger<RegisterCommandHandler>>();
    private readonly RegisterCommandHandler _handler;

    public RegisterCommandHandlerTests()
    {
        _handler = new RegisterCommandHandler(_userRepository, _passwordHasher, _logger);
    }

    [Fact]
    public async Task Handle_NewUser_CreatesAndReturnsResponse()
    {
        // Arrange
        _userRepository.ExistsByEmailAsync("new@example.com", Arg.Any<CancellationToken>())
            .Returns(false);
        _passwordHasher.Hash("password123").Returns("hashed-password");
        _userRepository.CreateAsync(Arg.Any<User>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => callInfo.Arg<User>());

        var command = new RegisterCommand
        {
            Email = "new@example.com",
            Password = "password123",
            FullName = "Jane Doe"
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Email.Should().Be("new@example.com");
        result.FullName.Should().Be("Jane Doe");
        result.UserId.Should().NotBeEmpty();

        await _userRepository.Received(1).CreateAsync(
            Arg.Is<User>(u =>
                u.Email == "new@example.com" &&
                u.PasswordHash == "hashed-password" &&
                u.FullName == "Jane Doe" &&
                u.Role == UserRole.Customer),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_DuplicateEmail_ThrowsArgumentException()
    {
        _userRepository.ExistsByEmailAsync("existing@example.com", Arg.Any<CancellationToken>())
            .Returns(true);

        var command = new RegisterCommand
        {
            Email = "existing@example.com",
            Password = "password123",
            FullName = "Test"
        };

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<ArgumentException>()
                 .WithMessage("*existing@example.com*already exists*");
    }
}
