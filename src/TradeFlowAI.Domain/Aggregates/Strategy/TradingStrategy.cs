using TradeFlowAI.Domain.Common;
using TradeFlowAI.Domain.Enums;
using TradeFlowAI.Domain.Events;

namespace TradeFlowAI.Domain.Aggregates.Strategy;

/// <summary>TradingStrategy Aggregate Root — AI strategy configuration.</summary>
public sealed class TradingStrategy : AggregateRoot<Guid>
{
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public StrategyType Type { get; private set; }
    public bool IsActive { get; private set; }
    public string ParametersJson { get; private set; } = "{}";
    public string[] SupportedSymbols { get; private set; } = [];
    public string CreatedByUserId { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    private TradingStrategy() { }

    public static TradingStrategy Create(
        Guid id, string name, string description,
        StrategyType type, string[] supportedSymbols,
        string createdByUserId, string parametersJson = "{}")
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(createdByUserId);

        return new TradingStrategy
        {
            Id = id,
            Name = name,
            Description = description,
            Type = type,
            SupportedSymbols = supportedSymbols,
            CreatedByUserId = createdByUserId,
            ParametersJson = parametersJson,
            IsActive = false,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void Activate()
    {
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateParameters(string parametersJson, string[] supportedSymbols)
    {
        ParametersJson = parametersJson;
        SupportedSymbols = supportedSymbols;
        UpdatedAt = DateTime.UtcNow;
    }

    public void EmitSignal(string symbol, string signal, decimal confidence)
    {
        RaiseDomainEvent(new StrategySignalGeneratedEvent(
            Guid.NewGuid(), DateTime.UtcNow, Id, symbol, signal, confidence));
    }
}
