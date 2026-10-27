namespace TradeFlowAI.Domain.Events;

public sealed record TradeExecutedEvent(
    Guid EventId,
    DateTime OccurredOn,
    Guid TradeId,
    Guid AccountId,
    string Symbol,
    decimal Quantity,
    decimal Price,
    string Direction) : IDomainEvent;

public sealed record TradeCancelledEvent(
    Guid EventId,
    DateTime OccurredOn,
    Guid TradeId,
    Guid AccountId,
    string Reason) : IDomainEvent;

public sealed record FundsDepositedEvent(
    Guid EventId,
    DateTime OccurredOn,
    Guid WalletId,
    Guid AccountId,
    decimal Amount,
    string Currency) : IDomainEvent;

public sealed record FundsWithdrawnEvent(
    Guid EventId,
    DateTime OccurredOn,
    Guid WalletId,
    Guid AccountId,
    decimal Amount,
    string Currency) : IDomainEvent;

public sealed record AccountCreatedEvent(
    Guid EventId,
    DateTime OccurredOn,
    Guid AccountId,
    string UserId) : IDomainEvent;

public sealed record PortfolioRebalancedEvent(
    Guid EventId,
    DateTime OccurredOn,
    Guid PortfolioId,
    Guid AccountId) : IDomainEvent;

public sealed record RiskLimitBreachedEvent(
    Guid EventId,
    DateTime OccurredOn,
    Guid AccountId,
    string LimitType,
    decimal CurrentValue,
    decimal Limit) : IDomainEvent;

public sealed record StrategySignalGeneratedEvent(
    Guid EventId,
    DateTime OccurredOn,
    Guid StrategyId,
    string Symbol,
    string Signal,
    decimal Confidence) : IDomainEvent;
