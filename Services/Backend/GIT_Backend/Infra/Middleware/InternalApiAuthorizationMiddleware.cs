using System.Security.Cryptography;
using System.Text;

namespace GIT_Backend.Infra.Middleware;

public class InternalApiAuthorizationMiddleware(
    RequestDelegate next,
    string internalApiKey,
    ILogger<InternalApiAuthorizationMiddleware> logger)
{
    private const string HeaderName = "X-Internal-Api-Key";

    public async Task InvokeAsync(HttpContext context)
    {
        if (!context.Request.Headers.TryGetValue(HeaderName, out var headerValues))
        {
            await WriteUnauthorizedAsync(context);
            return;
        }

        var requestApiKey = headerValues.Count == 1 ? headerValues[0] : null;
        if (string.IsNullOrWhiteSpace(requestApiKey) || !IsValidApiKey(requestApiKey))
        {
            logger.LogWarning(
                "Invalid internal api key request. Path: {Path}",
                context.Request.Path);

            await WriteUnauthorizedAsync(context);
            return;
        }

        await next(context);
    }

    private bool IsValidApiKey(string requestApiKey)
    {
        var expectedBytes = Encoding.UTF8.GetBytes(internalApiKey);
        var actualBytes = Encoding.UTF8.GetBytes(requestApiKey);

        return actualBytes.Length == expectedBytes.Length
            && CryptographicOperations.FixedTimeEquals(actualBytes, expectedBytes);
    }

    private static Task WriteUnauthorizedAsync(HttpContext context)
    {
        context.Response.StatusCode = StatusCodes.Status401Unauthorized;

        return context.Response.WriteAsync("Unauthorized");
    }
}
