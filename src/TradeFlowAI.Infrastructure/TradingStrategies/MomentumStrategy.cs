using TradeFlowAI.Application.Common.Interfaces;
using TradeFlowAI.Domain.Enums;

namespace TradeFlowAI.Infrastructure.TradingStrategies;

/// <summary>
/// Strategy Pattern implementation: Momentum Strategy.
/// Generates BUY signals when short-term moving average crosses above long-term moving average.
/// </summary>
public sealed class MomentumStrategy : IStrategyAlgorithm
{
    public StrategyType Type => StrategyType.Momentum;

    public Task<AiSignalResult> EvaluateAsync(
        string symbol,
        decimal currentPrice,
        IReadOnlyList<OhlcvBar> historicalBars,
        string parametersJson,
        CancellationToken ct = default)
    {
        if (historicalBars.Count < 20)
        {
            return Task.FromResult(new AiSignalResult(
                "HOLD", 0.5m, null, null, "Insufficient historical data for momentum calculation (requires >= 20 bars)."));
        }

        var shortPeriod = 5;
        var longPeriod = 20;

        var shortSma = historicalBars.TakeLast(shortPeriod).Average(b => b.Close);
        var longSma = historicalBars.TakeLast(longPeriod).Average(b => b.Close);

        if (shortSma > longSma * 1.01m)
        {
            return Task.FromResult(new AiSignalResult(
                "BUY",
                0.82m,
                1.0m,
                currentPrice,
                $"Bullish momentum crossover: {shortPeriod}-SMA ({shortSma:F2}) > {longPeriod}-SMA ({longSma:F2})"));
        }
        else if (shortSma < longSma * 0.99m)
        {
            return Task.FromResult(new AiSignalResult(
                "SELL",
                0.78m,
                1.0m,
                currentPrice,
                $"Bearish momentum breakdown: {shortPeriod}-SMA ({shortSma:F2}) < {longPeriod}-SMA ({longSma:F2})"));
        }

        return Task.FromResult(new AiSignalResult(
            "HOLD",
            0.60m,
            null,
            null,
            $"Consolidation: {shortPeriod}-SMA ({shortSma:F2}) near {longPeriod}-SMA ({longSma:F2})"));
    }
}
