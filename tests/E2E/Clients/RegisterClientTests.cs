using System.Net;
using System.Net.Http.Json;
using Application.UseCases.Clients.RegisterClient;
using E2E.Fixtures;
using FluentAssertions;
using Xunit;

namespace E2E.Clients;

public sealed class RegisterClientTests(WebAppFactory factory)
    : IClassFixture<WebAppFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task RegisterClient_WhenCommandIsValid_ShouldReturn201WithLocation()
    {
        var command = new RegisterClientCommand("John", "Doe", "john-e2e@example.com");

        var response = await _client.PostAsJsonAsync("/api/clients", command);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        response.Headers.Location.Should().NotBeNull();
    }

    [Fact]
    public async Task RegisterClient_WhenEmailAlreadyExists_ShouldReturn409()
    {
        var command = new RegisterClientCommand("John", "Doe", "john-conflict@example.com");

        await _client.PostAsJsonAsync("/api/clients", command);
        var response = await _client.PostAsJsonAsync("/api/clients", command);

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task RegisterClient_WhenCommandIsInvalid_ShouldReturn422WithErrors()
    {
        var command = new RegisterClientCommand("", "", "not-an-email");

        var response = await _client.PostAsJsonAsync("/api/clients", command);

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);

        var body = await response.Content.ReadFromJsonAsync<ProblemDetailsResponse>();
        body!.Errors.Should().ContainKey("FirstName");
        body.Errors.Should().ContainKey("LastName");
        body.Errors.Should().ContainKey("Email");
    }
}