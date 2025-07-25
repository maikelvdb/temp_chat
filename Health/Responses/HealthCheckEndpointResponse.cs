using System.Text.Json.Serialization;

namespace Omega.Shared.Observability.Health.Responses;

public class HealthCheckEndpointResponse
{
    [JsonPropertyName("{#NAME}")]
    public required string Name { get; set; }

    [JsonPropertyName("{#URL}")]
    public required Uri Url { get; set; }
}
