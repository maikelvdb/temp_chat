using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Omega.Shared.Observability.Middlewares;
using Prometheus;

namespace Omega.Shared.Observability.Extensions;

public static class ObservabilityExtensions
{
    public static WebApplicationBuilder RegisterObservability(this WebApplicationBuilder builder, Action<IHealthChecksBuilder>? healthChecks = null)
    {
        RegisterObservability(builder.Services, healthChecks);
        return builder;
    }

    public static IServiceCollection RegisterObservability(this IServiceCollection services, Action<IHealthChecksBuilder>? healthChecks = null)
    {
        var healthBuilder = services.AddHealthChecks();
        healthChecks?.Invoke(healthBuilder);

        return services;
    }

    public static void UseObservability(this WebApplication app)
    {
        app.MapMetrics(config => config.EnableOpenMetrics = true);

        app.UseMiddleware<MetricsAuthMiddleware>();
    }
}
