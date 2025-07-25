namespace Omega.Shared.Observability.Health.Responses;

public class HealthCheckStatusResponse
{
    public required string Name { get; set; }
    public required int Status { get; set; }
    public required string StatusDescription { get; set; }
    public required string? Message { get; set; }
    public required string? Exception { get; set; }
    public required double Duration { get; set; }

    public string[]? Tags { get; set; }
}
