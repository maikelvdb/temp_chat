using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace Omega.Shared.Observability.Middlewares;

public class MetricsAuthMiddleware(RequestDelegate next, IConfiguration config)
{
    public async Task InvokeAsync(HttpContext context)
    {
        if (!context.Request.Path.StartsWithSegments("/metrics", StringComparison.OrdinalIgnoreCase))
        {
            await next(context);
            return;
        }

        var apiKey = config["Metrics:ApiKey"];
        if (string.IsNullOrEmpty(apiKey))
        {
            await next(context);
            return;
        }

        if (TryGetApiKey(context, out var headerApiKey) && !headerApiKey.Equals(apiKey))
        {
            await WriteUnauthorized(context);
            return;
        }

        await next(context);
    }

    private static bool TryGetApiKey(HttpContext context, out string apiKey)
    {
        apiKey = string.Empty;

        var authHeader = context.Request.Headers.Authorization.ToString();
        if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Basic "))
        {
            return false;
        }

        var encodedCredentials = authHeader["Basic ".Length..].Trim();
        var decodedCredentials = Encoding.UTF8.GetString(Convert.FromBase64String(encodedCredentials));

        var credentials = decodedCredentials.Split(':');
        apiKey = credentials.Length > 1 ? credentials[1] : string.Empty;

        return !string.IsNullOrEmpty(apiKey);
    }

    private static async Task WriteUnauthorized(HttpContext context)
    {
        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        context.Response.ContentType = "application/problem+json";

        await context.Response.WriteAsync(
            JsonSerializer.Serialize(new ProblemDetails
            {
                Status = StatusCodes.Status401Unauthorized,
                Title = "Unauthorized",
                Detail = "Authorization failed",
                Instance = context.Request.Path,
            }), context.RequestAborted);
    }
}
