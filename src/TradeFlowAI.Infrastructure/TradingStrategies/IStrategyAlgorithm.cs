using TradeFlowAI.Application.Common.Interfaces;
using TradeFlowAI.Domain.Enums;

namespace TradeFlowAI.Infrastructure.TradingStrategies;

/// <summary>
/// Strategy Pattern Interface — defines the contract for swappable trading algorithm implementations.
/// Open/Closed Principle: New algorithms can be added without modifying existing code.
/// </summary>
public interface IStrategyAlgorithm
{
    StrategyType Type { get; }
    Task<AiSignalResult> EvaluateAsync(
        string symbol,
        decimal currentPrice,
        IReadOnlyList<OhlcvBar> historicalBars,
        string parametersJson,
        CancellationToken ct = default);
}
