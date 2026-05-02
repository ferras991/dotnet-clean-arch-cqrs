using Application.UseCases.Clients.RegisterClient;
using Domain.Entities;
using Domain.Errors;
using Domain.Repositories;
using FluentAssertions;
using NSubstitute;

namespace Unit.Application.UseCases.Clients.RegisterClient;

public sealed class RegisterClientCommandHandlerTests
{
    private readonly IClientRepository _repository;
    private readonly RegisterClientCommandHandler _handler;

    public RegisterClientCommandHandlerTests()
    {
        _repository = Substitute.For<IClientRepository>();
        _handler = new RegisterClientCommandHandler(_repository);
    }

    [Fact]
    public async Task Handle_WhenEmailIsUnique_ShouldInsertClientAndReturnId()
    {
        // Arrange
        var command = new RegisterClientCommand("John", "Doe", "john@example.com");

        _repository
            .ExistsByEmailAsync(command.Email, Arg.Any<CancellationToken>())
            .Returns(false);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();

        await _repository.Received(1).InsertAsync(
            Arg.Is<Client>(c => c.Email == command.Email),
            Arg.Any<CancellationToken>(),
            Arg.Any<System.Data.IDbTransaction?>());
    }

    [Fact]
    public async Task Handle_WhenEmailAlreadyExists_ShouldReturnConflictError()
    {
        // Arrange
        var command = new RegisterClientCommand("John", "Doe", "john@example.com");

        _repository
            .ExistsByEmailAsync(command.Email, Arg.Any<CancellationToken>())
            .Returns(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().BeOfType<ConflictError>();
        result.Error!.Code.Should().Be("Client.EmailInUse");

        await _repository.DidNotReceive().InsertAsync(
            Arg.Any<Client>(),
            Arg.Any<CancellationToken>(),
            Arg.Any<System.Data.IDbTransaction?>());
    }
}