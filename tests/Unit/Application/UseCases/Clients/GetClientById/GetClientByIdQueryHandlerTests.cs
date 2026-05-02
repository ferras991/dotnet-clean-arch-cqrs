using Application.UseCases.Clients.GetClientById;
using Domain.Entities;
using Domain.Errors;
using Domain.Repositories;
using FluentAssertions;
using NSubstitute;

namespace Unit.Application.UseCases.Clients.GetClientById;

public sealed class GetClientByIdQueryHandlerTests
{
    private readonly IClientRepository _repository;
    private readonly GetClientByIdQueryHandler _handler;

    public GetClientByIdQueryHandlerTests()
    {
        _repository = Substitute.For<IClientRepository>();
        _handler = new GetClientByIdQueryHandler(_repository);
    }

    [Fact]
    public async Task Handle_WhenClientExists_ShouldReturnClientResponse()
    {
        // Arrange
        var client = Client.Create("John", "Doe", "john@example.com");
        var query = new GetClientByIdQuery(client.Id);

        _repository
            .GetByIdAsync(client.Id, Arg.Any<CancellationToken>())
            .Returns(client);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(client.Id);
        result.Value.FullName.Should().Be("John Doe");
        result.Value.Email.Should().Be("john@example.com");
    }

    [Fact]
    public async Task Handle_WhenClientDoesNotExist_ShouldReturnNotFoundError()
    {
        // Arrange
        var query = new GetClientByIdQuery(Guid.NewGuid());

        _repository
            .GetByIdAsync(query.ClientId, Arg.Any<CancellationToken>())
            .Returns((Client?)null);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().BeOfType<NotFoundError>();
    }
}