using Microsoft.Extensions.Logging;
using TradeFlowAI.Application.Common.Interfaces;

namespace TradeFlowAI.Infrastructure.ExternalServices.AI;

/// <summary>
/// Decorator Pattern: Wraps IAiStrategyEngine to provide automatic transient fault retries.
/// </summary>
public sealed class AiStrategyEngineRetryDecorator(
    IAiStrategyEngine inner,
    ILogger<AiStrategyEngineRetryDecorator> logger,
    int maxRetries = 3)
    : IAiStrategyEngine
{
    public async Task<AiSignalResult> GenerateSignalAsync(AiSignalRequest request, CancellationToken ct = default)
    {
        int attempt = 0;
        while (true)
        {
            attempt++;
            try
            {
                return await inner.GenerateSignalAsync(request, ct);
            }
            catch (Exception ex) when (attempt <= maxRetries && !ct.IsCancellationRequested)
            {
                logger.LogWarning(ex, "Attempt {Attempt}/{MaxRetries} failed for AI Signal generation. Retrying...", attempt, maxRetries);
                await Task.Delay(100 * attempt, ct);
            }
        }
    }

    public async Task<BacktestResult> RunBacktestAsync(BacktestRequest request, CancellationToken ct = default)
    {
        int attempt = 0;
        while (true)
        {
            attempt++;
            try
            {
                return await inner.RunBacktestAsync(request, ct);
            }
            catch (Exception ex) when (attempt <= maxRetries && !ct.IsCancellationRequested)
            {
                logger.LogWarning(ex, "Attempt {Attempt}/{MaxRetries} failed for backtest. Retrying...", attempt, maxRetries);
                await Task.Delay(100 * attempt, ct);
            }
        }
    }
}
