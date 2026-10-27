using TradeFlowAI.Domain.Enums;

namespace TradeFlowAI.Infrastructure.TradingStrategies;

/// <summary>
/// Factory Pattern: Resolves the appropriate IStrategyAlgorithm based on StrategyType.
/// Allows dynamic algorithm selection at runtime without tight coupling.
/// </summary>
public interface IStrategyFactory
{
    IStrategyAlgorithm GetStrategy(StrategyType type);
}

public sealed class StrategyFactory(IEnumerable<IStrategyAlgorithm> strategies) : IStrategyFactory
{
    private readonly Dictionary<StrategyType, IStrategyAlgorithm> _strategies =
        strategies.ToDictionary(s => s.Type);

    public IStrategyAlgorithm GetStrategy(StrategyType type)
    {
        if (_strategies.TryGetValue(type, out var strategy))
        {
            return strategy;
        }

        // Default fallback to AiDriven
        return _strategies.GetValueOrDefault(StrategyType.AiDriven)
            ?? throw new NotSupportedException($"Strategy algorithm for type '{type}' is not registered.");
    }
}
