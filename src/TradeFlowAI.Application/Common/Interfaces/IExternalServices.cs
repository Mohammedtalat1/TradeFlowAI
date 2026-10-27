namespace TradeFlowAI.Application.Common.Interfaces;

/// <summary>
/// AI Strategy Engine contract — Strategy pattern.
/// The infrastructure layer provides the concrete implementation 
/// (ML model, GPT-based, rule-based, etc.) without the Application knowing which.
/// </summary>
public interface IAiStrategyEngine
{
    /// <summary>Generate a trading signal for the given symbol using the strategy config.</summary>
    Task<AiSignalResult> GenerateSignalAsync(
        AiSignalRequest request,
        CancellationToken ct = default);

    /// <summary>Run a backtest of a strategy against historical data.</summary>
    Task<BacktestResult> RunBacktestAsync(
        BacktestRequest request,
        CancellationToken ct = default);
}

public sealed record AiSignalRequest(
    Guid StrategyId,
    string Symbol,
    string ParametersJson,
    decimal CurrentPrice,
    IReadOnlyList<OhlcvBar> HistoricalBars);

public sealed record AiSignalResult(
    string Signal,        // "BUY", "SELL", "HOLD"
    decimal Confidence,   // 0.0 - 1.0
    decimal? SuggestedQuantity,
    decimal? SuggestedLimitPrice,
    string Rationale);

public sealed record OhlcvBar(
    DateTime Timestamp,
    decimal Open,
    decimal High,
    decimal Low,
    decimal Close,
    decimal Volume);

public sealed record BacktestRequest(
    Guid StrategyId,
    string Symbol,
    string ParametersJson,
    DateTime From,
    DateTime To,
    decimal InitialCapital);

public sealed record BacktestResult(
    Guid RunId,
    decimal FinalCapital,
    decimal TotalReturn,
    decimal MaxDrawdown,
    decimal SharpeRatio,
    int TotalTrades,
    int WinningTrades,
    int LosingTrades,
    string ResultsJson);

/// <summary>Market data provider contract — Adapter pattern.</summary>
public interface IMarketDataService
{
    Task<decimal> GetCurrentPriceAsync(string symbol, CancellationToken ct = default);
    Task<IReadOnlyList<OhlcvBar>> GetHistoricalBarsAsync(string symbol, string interval, int limit, CancellationToken ct = default);
    Task<MarketSummary> GetMarketSummaryAsync(string symbol, CancellationToken ct = default);
    Task<IReadOnlyList<string>> GetSupportedSymbolsAsync(CancellationToken ct = default);
}

public sealed record MarketSummary(
    string Symbol,
    decimal Price,
    decimal Change24h,
    decimal ChangePercent24h,
    decimal Volume24h,
    decimal High24h,
    decimal Low24h);

/// <summary>Payment gateway contract — Adapter pattern for deposits/withdrawals.</summary>
public interface IPaymentGateway
{
    Task<PaymentResult> ProcessDepositAsync(Guid accountId, decimal amount, string currency, string externalRef, CancellationToken ct = default);
    Task<PaymentResult> ProcessWithdrawalAsync(Guid accountId, decimal amount, string currency, string bankDetails, CancellationToken ct = default);
}

public sealed record PaymentResult(bool Success, string TransactionId, string? ErrorMessage);
