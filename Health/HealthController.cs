using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using Omega.Shared.Observability.Health.Responses;

namespace Omega.Shared.Observability.Health;

[ApiController]
[AllowAnonymous]
[Route("api/[controller]")]
public class HealthController(IOptions<HealthCheckServiceOptions> options, HealthCheckService healthCheckService)
    : ControllerBase
{
    [HttpGet]
    public ActionResult Discover()
    {
        var baseUrl = GetBaseUrl();
        var checks = options.Value.Registrations.Select(r => new HealthCheckEndpointResponse
        {
            Name = r.Name,
            Url = new Uri(baseUrl, r.Name.Normalize()),
        });

        return Ok(checks);
    }

    [HttpGet("{CheckName}")]
    public async Task<ActionResult> Check([FromRoute(Name = "CheckName")] string checkName, CancellationToken ct)
    {
        var report = await healthCheckService.CheckHealthAsync(r => r.Name.Equals(checkName, StringComparison.OrdinalIgnoreCase) , ct);
        if (!report.Entries.Any())
        {
            return NotFound();
        }

        var entry = report.Entries[checkName];
        return Ok(new HealthCheckStatusResponse
        {
            Name = checkName,
            Status = GetStatusCode(entry.Status),
            StatusDescription = Enum.GetName(entry.Status) ?? string.Empty,
            Message = entry.Description,
            Exception = entry.Exception?.Message,
            Duration = entry.Duration.TotalSeconds,
            Tags = [.. entry.Tags],
        });
    }

    private Uri GetBaseUrl()
    {
        var controllerName = ControllerContext.ActionDescriptor.ControllerName;
        var uriBuilder = new UriBuilder(Request.Scheme, Request.Host.Host)
        {
            Port = Request.Host.Port ?? 80,
            Path = $"api/{controllerName}/"
        };

        return uriBuilder.Uri;
    }

    private static int GetStatusCode(HealthStatus status)
    {
        return status switch
        {
            HealthStatus.Healthy => (int)HealthCheckStatusCode.Healthy,
            HealthStatus.Degraded => (int)HealthCheckStatusCode.Degraded,
            HealthStatus.Unhealthy => (int)HealthCheckStatusCode.Unhealthy,
            _ => (int)HealthCheckStatusCode.Unknown,
        };
    }
}
