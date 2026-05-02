using Domain.Entities;
using FluentAssertions;
using Infrastructure.Repositories;
using Integration.Fixtures;

namespace Integration.Infrastructure.Repositories;

public sealed class ClientRepositoryTests(DatabaseFixture fixture)
    : IClassFixture<DatabaseFixture>
{
    private readonly ClientRepository _repository = new(fixture.DbContext);

    [Fact]
    public async Task InsertAsync_ShouldPersistClient()
    {
        // Arrange
        var client = Client.Create("John", "Doe", "john-integration@example.com");

        // Act
        await _repository.InsertAsync(client, CancellationToken.None);

        // Assert
        var persisted = await _repository.GetByIdAsync(client.Id, CancellationToken.None);
        persisted.Should().NotBeNull();
        persisted!.Email.Should().Be(client.Email);
        persisted.FullName.Should().Be("John Doe");
    }

    [Fact]
    public async Task GetByIdAsync_WhenClientDoesNotExist_ShouldReturnNull()
    {
        var result = await _repository.GetByIdAsync(Guid.NewGuid(), CancellationToken.None);

        result.Should().BeNull();
    }

    [Fact]
    public async Task ExistsByEmailAsync_WhenEmailExists_ShouldReturnTrue()
    {
        // Arrange
        var client = Client.Create("Jane", "Doe", "jane-exists@example.com");
        await _repository.InsertAsync(client, CancellationToken.None);

        // Act
        var exists = await _repository.ExistsByEmailAsync(client.Email, CancellationToken.None);

        // Assert
        exists.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsByEmailAsync_WhenEmailDoesNotExist_ShouldReturnFalse()
    {
        var exists = await _repository.ExistsByEmailAsync("ghost@example.com", CancellationToken.None);

        exists.Should().BeFalse();
    }
}