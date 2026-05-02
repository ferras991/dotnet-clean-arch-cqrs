using System.Net;
using System.Net.Http.Json;
using Application.UseCases.Clients.GetClientById;
using Application.UseCases.Clients.RegisterClient;
using E2E.Fixtures;
using FluentAssertions;
using Xunit;

namespace E2E.Clients;

public sealed class GetClientByIdTests(WebAppFactory factory)
    : IClassFixture<WebAppFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task GetClientById_WhenClientExists_ShouldReturn200WithClient()
    {
        // Arrange
        var command = new RegisterClientCommand("Jane", "Doe", "jane-e2e@example.com");
        var createResponse = await _client.PostAsJsonAsync("/api/clients", command);
        var id = ExtractIdFromLocation(createResponse.Headers.Location!);

        // Act
        var response = await _client.GetAsync($"/api/clients/{id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<ClientResponse>();
        body!.FullName.Should().Be("Jane Doe");
        body.Email.Should().Be("jane-e2e@example.com");
    }

    [Fact]
    public async Task GetClientById_WhenClientDoesNotExist_ShouldReturn404()
    {
        var response = await _client.GetAsync($"/api/clients/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    private static Guid ExtractIdFromLocation(Uri location)
    {
        var segments = location.Segments;
        return Guid.Parse(segments[^1]);
    }
}