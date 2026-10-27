using System.Diagnostics;
using Microsoft.Extensions.Logging;
using TradeFlowAI.Application.Common.Interfaces;

namespace TradeFlowAI.Infrastructure.ExternalServices.AI;

/// <summary>
/// Decorator Pattern: Wraps IAiStrategyEngine to inject performance timing and structured diagnostic logs.
/// Follows Single Responsibility and Open/Closed principles.
/// </summary>
public sealed class AiStrategyEngineLoggingDecorator(
    IAiStrategyEngine inner,
    ILogger<AiStrategyEngineLoggingDecorator> logger)
    : IAiStrategyEngine
{
    public async Task<AiSignalResult> GenerateSignalAsync(AiSignalRequest request, CancellationToken ct = default)
    {
        var sw = Stopwatch.StartNew();
        logger.LogInformation("AI Strategy Engine generating signal for {Symbol} on Strategy {StrategyId}",
            request.Symbol, request.StrategyId);

        try
        {
            var result = await inner.GenerateSignalAsync(request, ct);
            sw.Stop();
            logger.LogInformation("AI Signal generated for {Symbol}: {Signal} with {Confidence:P1} confidence in {ElapsedMs}ms",
                request.Symbol, result.Signal, result.Confidence, sw.ElapsedMilliseconds);
            return result;
        }
        catch (Exception ex)
        {
            sw.Stop();
            logger.LogError(ex, "Failed to generate AI signal for {Symbol} after {ElapsedMs}ms",
                request.Symbol, sw.ElapsedMilliseconds);
            throw;
        }
    }

    public async Task<BacktestResult> RunBacktestAsync(BacktestRequest request, CancellationToken ct = default)
    {
        var sw = Stopwatch.StartNew();
        logger.LogInformation("Starting backtest for Strategy {StrategyId} on {Symbol} from {From} to {To}",
            request.StrategyId, request.Symbol, request.From, request.To);

        try
        {
            var result = await inner.RunBacktestAsync(request, ct);
            sw.Stop();
            logger.LogInformation("Backtest completed in {ElapsedMs}ms: FinalCapital=${FinalCapital}, Return={Return:F1}%, Sharpe={Sharpe}",
                sw.ElapsedMilliseconds, result.FinalCapital, result.TotalReturn, result.SharpeRatio);
            return result;
        }
        catch (Exception ex)
        {
            sw.Stop();
            logger.LogError(ex, "Backtest failed for {StrategyId} after {ElapsedMs}ms",
                request.StrategyId, sw.ElapsedMilliseconds);
            throw;
        }
    }
}
