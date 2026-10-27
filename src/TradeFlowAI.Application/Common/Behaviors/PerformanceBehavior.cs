using MediatR;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace TradeFlowAI.Application.Common.Behaviors;

/// <summary>
/// MediatR Pipeline Behavior — Performance monitoring.
/// Logs a warning if a request takes longer than the configured threshold.
/// </summary>
public sealed class PerformanceBehavior<TRequest, TResponse>(
    ILogger<PerformanceBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private const int SlowRequestThresholdMs = 500;

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken ct)
    {
        var sw = Stopwatch.StartNew();

        var response = await next();

        sw.Stop();

        if (sw.ElapsedMilliseconds > SlowRequestThresholdMs)
        {
            logger.LogWarning(
                "[TradeFlowAI] SLOW REQUEST detected: {RequestName} took {ElapsedMs}ms — {@Request}",
                typeof(TRequest).Name, sw.ElapsedMilliseconds, request);
        }

        return response;
    }
}
