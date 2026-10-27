using TradeFlowAI.Application.Common.Interfaces;
using TradeFlowAI.Domain.Enums;

namespace TradeFlowAI.Infrastructure.TradingStrategies;

/// <summary>
/// Strategy Pattern implementation: Mean Reversion Strategy.
/// Uses standard deviation / Bollinger Bands concept: buys when oversold below mean, sells when overbought.
/// </summary>
public sealed class MeanReversionStrategy : IStrategyAlgorithm
{
    public StrategyType Type => StrategyType.MeanReversion;

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
                "HOLD", 0.5m, null, null, "Insufficient data for mean reversion calculation."));
        }

        var closes = historicalBars.TakeLast(20).Select(b => (double)b.Close).ToList();
        var mean = closes.Average();
        var variance = closes.Average(v => Math.Pow(v - mean, 2));
        var stdDev = Math.Sqrt(variance);

        var current = (double)currentPrice;
        var zScore = (current - mean) / (stdDev > 0 ? stdDev : 1.0);

        if (zScore < -2.0)
        {
            return Task.FromResult(new AiSignalResult(
                "BUY",
                0.88m,
                1.5m,
                currentPrice,
                $"Oversold signal: Z-Score = {zScore:F2} (Price {currentPrice:F2} is 2+ StdDev below mean {mean:F2})"));
        }
        else if (zScore > 2.0)
        {
            return Task.FromResult(new AiSignalResult(
                "SELL",
                0.85m,
                1.5m,
                currentPrice,
                $"Overbought signal: Z-Score = {zScore:F2} (Price {currentPrice:F2} is 2+ StdDev above mean {mean:F2})"));
        }

        return Task.FromResult(new AiSignalResult(
            "HOLD",
            0.65m,
            null,
            null,
            $"Price within normal bounds: Z-Score = {zScore:F2}"));
    }
}
