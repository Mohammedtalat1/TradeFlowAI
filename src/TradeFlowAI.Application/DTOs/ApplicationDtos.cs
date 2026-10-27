using TradeFlowAI.Domain.Enums;

namespace TradeFlowAI.Application.DTOs;

public record AccountDto(
    Guid Id,
    string UserId,
    string Name,
    AccountStatus Status,
    bool IsAutoTradingEnabled,
    Guid? ActiveStrategyId,
    DateTime CreatedAt);

public record WalletDto(
    Guid Id,
    Guid AccountId,
    decimal Balance,
    decimal ReservedBalance,
    decimal AvailableBalance,
    string Currency);

public record TransactionDto(
    Guid Id,
    TransactionType Type,
    decimal Amount,
    string Currency,
    string Reference,
    DateTime CreatedAt);

public record PortfolioDto(
    Guid Id,
    Guid AccountId,
    decimal TotalValue,
    decimal TotalCost,
    decimal UnrealizedPnl,
    decimal RealizedPnl,
    IReadOnlyList<PositionDto> Positions);

public record PositionDto(
    Guid Id,
    string Symbol,
    decimal Quantity,
    decimal AverageCost,
    decimal CurrentPrice,
    decimal MarketValue,
    decimal UnrealizedPnl);

public record TradeDto(
    Guid Id,
    Guid AccountId,
    string Symbol,
    TradeDirection Direction,
    OrderType OrderType,
    decimal Quantity,
    decimal FilledQuantity,
    decimal? LimitPrice,
    decimal? ExecutionPrice,
    TradeStatus Status,
    bool IsAiInitiated,
    Guid? StrategyId,
    string? CancellationReason,
    DateTime CreatedAt,
    DateTime? ExecutedAt);

public record TradingStrategyDto(
    Guid Id,
    string Name,
    string Description,
    StrategyType Type,
    bool IsActive,
    string[] SupportedSymbols,
    DateTime CreatedAt);

public record MarketSummaryDto(
    string Symbol,
    decimal Price,
    decimal Change24h,
    decimal ChangePercent24h,
    decimal Volume24h,
    decimal High24h,
    decimal Low24h);

public record RiskMetricsDto(
    Guid AccountId,
    decimal CurrentDrawdownPercent,
    decimal DailyLossPercent,
    int OpenTradeCount,
    decimal TotalExposure,
    bool IsCircuitBreakerActive);

public record WebhookDto(
    Guid Id,
    string Url,
    string[] Events,
    bool IsActive,
    DateTime CreatedAt);

public record ApiKeyDto(
    Guid Id,
    string KeyPrefix,
    string[] Scopes,
    DateTime CreatedAt,
    DateTime? ExpiresAt);

public record PagedResult<T>(
    IReadOnlyList<T> Items,
    int TotalCount,
    int Page,
    int PageSize)
{
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    public bool HasNextPage => Page < TotalPages;
    public bool HasPreviousPage => Page > 1;
}
