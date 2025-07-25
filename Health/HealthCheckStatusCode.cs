namespace Omega.Shared.Observability.Health;

public enum HealthCheckStatusCode
{
    Healthy = 0,
    Degraded = 1,
    Unhealthy = 2,

    Unknown = 3
}
