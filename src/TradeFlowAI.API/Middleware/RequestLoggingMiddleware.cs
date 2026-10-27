using System.Diagnostics;

namespace TradeFlowAI.API.Middleware;

public sealed class RequestLoggingMiddleware(
    RequestDelegate next,
    ILogger<RequestLoggingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var sw = Stopwatch.StartNew();
        var path = context.Request.Path;
        var method = context.Request.Method;

        try
        {
            await next(context);
            sw.Stop();

            logger.LogInformation("HTTP {Method} {Path} responded {StatusCode} in {ElapsedMs}ms",
                method, path, context.Response.StatusCode, sw.ElapsedMilliseconds);
        }
        catch (Exception)
        {
            sw.Stop();
            logger.LogWarning("HTTP {Method} {Path} failed after {ElapsedMs}ms",
                method, path, sw.ElapsedMilliseconds);
            throw;
        }
    }
}
