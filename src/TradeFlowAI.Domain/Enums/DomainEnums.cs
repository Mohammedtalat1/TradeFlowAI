namespace TradeFlowAI.Domain.Enums;

public enum AccountStatus
{
    PendingKyc,
    Active,
    Suspended,
    Closed
}

public enum TradeStatus
{
    // State machine: valid transitions enforced in Trade aggregate
    Pending,
    Submitted,
    PartiallyFilled,
    Filled,
    Cancelled,
    Rejected,
    Expired
}

public enum TradeDirection
{
    Buy,
    Sell
}

public enum OrderType
{
    Market,
    Limit,
    StopLoss,
    TakeProfit,
    TrailingStop
}

public enum TransactionType
{
    Deposit,
    Withdrawal,
    Fee,
    PnlCredit,
    PnlDebit,
    Transfer
}

public enum StrategyType
{
    Momentum,
    MeanReversion,
    Arbitrage,
    AiDriven,
    Custom
}

public enum NotificationChannel
{
    Email,
    Sms,
    Push,
    Webhook
}

public enum WebhookEventType
{
    TradeExecuted,
    TradeCancelled,
    FundsDeposited,
    FundsWithdrawn,
    RiskLimitBreached,
    PortfolioRebalanced,
    StrategySignalGenerated
}
