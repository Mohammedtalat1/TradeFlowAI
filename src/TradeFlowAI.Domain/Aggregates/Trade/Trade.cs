using TradeFlowAI.Domain.Common;
using TradeFlowAI.Domain.Enums;
using TradeFlowAI.Domain.Events;
using TradeFlowAI.Domain.Exceptions;
using TradeFlowAI.Domain.ValueObjects;

namespace TradeFlowAI.Domain.Aggregates.Trade;

/// <summary>
/// Trade Aggregate Root — implements the State Machine pattern.
/// Valid state transitions:
///   Pending → Submitted → PartiallyFilled → Filled
///   Pending → Cancelled
///   Submitted → Cancelled | Rejected | Expired
///   PartiallyFilled → Filled | Cancelled
/// </summary>
public sealed class Trade : AggregateRoot<Guid>
{
    public Guid AccountId { get; private set; }
    public string Symbol { get; private set; } = string.Empty;
    public TradeDirection Direction { get; private set; }
    public OrderType OrderType { get; private set; }
    public decimal Quantity { get; private set; }
    public decimal FilledQuantity { get; private set; }
    public decimal? LimitPrice { get; private set; }
    public decimal? ExecutionPrice { get; private set; }
    public TradeStatus Status { get; private set; }
    public bool IsAiInitiated { get; private set; }
    public Guid? StrategyId { get; private set; }
    public string? CancellationReason { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? ExecutedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    private Trade() { }

    // Factory Method — creates the correct trade from parameters
    public static Trade Create(
        Guid id,
        Guid accountId,
        string symbol,
        TradeDirection direction,
        OrderType orderType,
        decimal quantity,
        decimal? limitPrice = null,
        Guid? strategyId = null,
        bool isAiInitiated = false)
    {
        if (quantity <= 0) throw new DomainException("Trade quantity must be positive.");
        if (orderType == OrderType.Limit && limitPrice is null)
            throw new DomainException("Limit orders require a limit price.");

        return new Trade
        {
            Id = id,
            AccountId = accountId,
            Symbol = symbol.ToUpperInvariant(),
            Direction = direction,
            OrderType = orderType,
            Quantity = quantity,
            FilledQuantity = 0,
            LimitPrice = limitPrice,
            Status = TradeStatus.Pending,
            StrategyId = strategyId,
            IsAiInitiated = isAiInitiated,
            CreatedAt = DateTime.UtcNow
        };
    }

    // ── State machine transitions ──────────────────────────────────────────

    public void Submit()
    {
        EnsureStatus(TradeStatus.Pending, "Only pending trades can be submitted.");
        Status = TradeStatus.Submitted;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Fill(decimal executionPrice, decimal filledQuantity)
    {
        if (Status is not (TradeStatus.Submitted or TradeStatus.PartiallyFilled))
            throw new InvalidTradeStateException(Status, TradeStatus.Filled);

        if (executionPrice <= 0) throw new DomainException("Execution price must be positive.");
        if (filledQuantity <= 0 || filledQuantity > (Quantity - FilledQuantity))
            throw new DomainException("Invalid fill quantity.");

        ExecutionPrice = executionPrice;
        FilledQuantity += filledQuantity;
        ExecutedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;

        Status = FilledQuantity >= Quantity ? TradeStatus.Filled : TradeStatus.PartiallyFilled;

        if (Status == TradeStatus.Filled)
        {
            RaiseDomainEvent(new TradeExecutedEvent(
                Guid.NewGuid(), DateTime.UtcNow,
                Id, AccountId, Symbol, Quantity, executionPrice, Direction.ToString()));
        }
    }

    public void Cancel(string reason)
    {
        if (Status is TradeStatus.Filled or TradeStatus.Rejected or TradeStatus.Expired)
            throw new InvalidTradeStateException(Status, TradeStatus.Cancelled);

        Status = TradeStatus.Cancelled;
        CancellationReason = reason;
        UpdatedAt = DateTime.UtcNow;

        RaiseDomainEvent(new TradeCancelledEvent(
            Guid.NewGuid(), DateTime.UtcNow, Id, AccountId, reason));
    }

    public void Reject(string reason)
    {
        EnsureStatus(TradeStatus.Submitted, "Only submitted trades can be rejected.");
        Status = TradeStatus.Rejected;
        CancellationReason = reason;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Expire()
    {
        if (Status is not (TradeStatus.Submitted or TradeStatus.PartiallyFilled))
            throw new InvalidTradeStateException(Status, TradeStatus.Expired);
        Status = TradeStatus.Expired;
        UpdatedAt = DateTime.UtcNow;
    }

    public decimal RemainingQuantity => Quantity - FilledQuantity;
    public bool IsOpen => Status is TradeStatus.Pending or TradeStatus.Submitted or TradeStatus.PartiallyFilled;

    private void EnsureStatus(TradeStatus expected, string message)
    {
        if (Status != expected) throw new DomainException(message);
    }
}
