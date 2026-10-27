namespace TradeFlowAI.Domain.Exceptions;

public class DomainException : Exception
{
    public DomainException(string message) : base(message) { }
    public DomainException(string message, Exception inner) : base(message, inner) { }
}

public class InsufficientFundsException : DomainException
{
    public decimal Available { get; }
    public decimal Required { get; }

    public InsufficientFundsException(decimal available, decimal required)
        : base($"Insufficient funds. Available: {available}, Required: {required}")
    {
        Available = available;
        Required = required;
    }
}

public class RiskLimitExceededException : DomainException
{
    public string LimitType { get; }
    public decimal CurrentValue { get; }
    public decimal Limit { get; }

    public RiskLimitExceededException(string limitType, decimal current, decimal limit)
        : base($"Risk limit exceeded. {limitType}: {current} > {limit}")
    {
        LimitType = limitType;
        CurrentValue = current;
        Limit = limit;
    }
}

public class InvalidTradeStateException : DomainException
{
    public InvalidTradeStateException(object currentStatus, object attemptedStatus)
        : base($"Invalid trade state transition: {currentStatus} → {attemptedStatus}") { }
}
