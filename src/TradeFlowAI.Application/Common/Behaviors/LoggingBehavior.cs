using MediatR;
using Microsoft.Extensions.Logging;

namespace TradeFlowAI.Application.Common.Behaviors;

/// <summary>
/// MediatR Pipeline Behavior — Logging.
/// Logs every request/response flowing through the pipeline without touching handler code.
/// Demonstrates the Decorator / Pipeline pattern.
/// </summary>
public sealed class LoggingBehavior<TRequest, TResponse>(
    ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken ct)
    {
        var requestName = typeof(TRequest).Name;

        logger.LogInformation("[TradeFlowAI] Handling {RequestName} {@Request}", requestName, request);

        var response = await next();

        logger.LogInformation("[TradeFlowAI] Handled {RequestName} successfully", requestName);

        return response;
    }
}
