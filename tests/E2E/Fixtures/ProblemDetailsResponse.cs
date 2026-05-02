using System.Text.Json.Serialization;

namespace E2E.Fixtures;

public sealed class ProblemDetailsResponse
{
    [JsonPropertyName("title")]
    public string Title { get; init; } = null!;

    [JsonPropertyName("detail")]
    public string Detail { get; init; } = null!;

    [JsonPropertyName("status")]
    public int Status { get; init; }

    [JsonPropertyName("errors")]
    public Dictionary<string, string[]> Errors { get; init; } = [];
}