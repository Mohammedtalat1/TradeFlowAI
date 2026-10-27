using TradeFlowAI.Domain.Common;

namespace TradeFlowAI.Domain.ValueObjects;

/// <summary>Encapsulates user risk configuration as an immutable value object.</summary>
public sealed class RiskProfile : ValueObject
{
    public decimal MaxDrawdownPercent { get; }       // Max allowed portfolio drawdown %
    public decimal MaxPositionSizePercent { get; }   // Max single position as % of portfolio
    public decimal DailyLossLimitPercent { get; }    // Stop trading after X% daily loss
    public int MaxOpenTrades { get; }                // Max concurrent open trades
    public decimal LeverageMultiplier { get; }       // e.g., 1.0 = no leverage, 2.0 = 2x

    private RiskProfile() { }

    public RiskProfile(
        decimal maxDrawdownPercent,
        decimal maxPositionSizePercent,
        decimal dailyLossLimitPercent,
        int maxOpenTrades,
        decimal leverageMultiplier)
    {
        if (maxDrawdownPercent is <= 0 or > 100) throw new ArgumentOutOfRangeException(nameof(maxDrawdownPercent));
        if (maxPositionSizePercent is <= 0 or > 100) throw new ArgumentOutOfRangeException(nameof(maxPositionSizePercent));
        if (dailyLossLimitPercent is <= 0 or > 100) throw new ArgumentOutOfRangeException(nameof(dailyLossLimitPercent));
        if (maxOpenTrades <= 0) throw new ArgumentOutOfRangeException(nameof(maxOpenTrades));
        if (leverageMultiplier < 1) throw new ArgumentOutOfRangeException(nameof(leverageMultiplier));

        MaxDrawdownPercent = maxDrawdownPercent;
        MaxPositionSizePercent = maxPositionSizePercent;
        DailyLossLimitPercent = dailyLossLimitPercent;
        MaxOpenTrades = maxOpenTrades;
        LeverageMultiplier = leverageMultiplier;
    }

    /// <summary>Conservative default suitable for beginners.</summary>
    public static RiskProfile Conservative() => new(10, 5, 3, 5, 1.0m);

    /// <summary>Moderate default with balanced risk/reward.</summary>
    public static RiskProfile Moderate() => new(20, 15, 8, 15, 2.0m);

    /// <summary>Aggressive for experienced traders.</summary>
    public static RiskProfile Aggressive() => new(40, 30, 20, 50, 5.0m);

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return MaxDrawdownPercent;
        yield return MaxPositionSizePercent;
        yield return DailyLossLimitPercent;
        yield return MaxOpenTrades;
        yield return LeverageMultiplier;
    }
}
