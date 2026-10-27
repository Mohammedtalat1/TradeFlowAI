using TradeFlowAI.Domain.Common;
using TradeFlowAI.Domain.Enums;
using TradeFlowAI.Domain.Events;
using TradeFlowAI.Domain.Exceptions;
using TradeFlowAI.Domain.ValueObjects;

namespace TradeFlowAI.Domain.Aggregates.Fund;

/// <summary>
/// Wallet Aggregate Root.
/// Owns all fund movements. Enforces balance invariants.
/// </summary>
public sealed class Wallet : AggregateRoot<Guid>
{
    private readonly List<Transaction> _transactions = [];

    public Guid AccountId { get; private set; }
    public Money Balance { get; private set; } = Money.Zero("USD");
    public Money ReservedBalance { get; private set; } = Money.Zero("USD");
    public Money AvailableBalance => Balance.Subtract(ReservedBalance);
    public string Currency { get; private set; } = "USD";
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    public IReadOnlyList<Transaction> Transactions => _transactions.AsReadOnly();

    private Wallet() { }

    public static Wallet Create(Guid id, Guid accountId, string currency = "USD")
    {
        return new Wallet
        {
            Id = id,
            AccountId = accountId,
            Currency = currency.ToUpperInvariant(),
            Balance = Money.Zero(currency),
            ReservedBalance = Money.Zero(currency),
            CreatedAt = DateTime.UtcNow
        };
    }

    public void Deposit(decimal amount, string reference)
    {
        if (amount <= 0) throw new DomainException("Deposit amount must be positive.");

        var money = Money.Of(amount, Currency);
        Balance = Balance.Add(money);

        var tx = Transaction.Create(Guid.NewGuid(), Id, TransactionType.Deposit, money, reference);
        _transactions.Add(tx);
        UpdatedAt = DateTime.UtcNow;

        RaiseDomainEvent(new FundsDepositedEvent(
            Guid.NewGuid(), DateTime.UtcNow, Id, AccountId, amount, Currency));
    }

    public void Withdraw(decimal amount, string reference)
    {
        var money = Money.Of(amount, Currency);
        if (!AvailableBalance.IsGreaterThanOrEqual(money))
            throw new InsufficientFundsException(AvailableBalance.Amount, amount);

        Balance = Balance.Subtract(money);

        var tx = Transaction.Create(Guid.NewGuid(), Id, TransactionType.Withdrawal, money, reference);
        _transactions.Add(tx);
        UpdatedAt = DateTime.UtcNow;

        RaiseDomainEvent(new FundsWithdrawnEvent(
            Guid.NewGuid(), DateTime.UtcNow, Id, AccountId, amount, Currency));
    }

    public void Reserve(decimal amount)
    {
        var money = Money.Of(amount, Currency);
        if (!AvailableBalance.IsGreaterThanOrEqual(money))
            throw new InsufficientFundsException(AvailableBalance.Amount, amount);
        ReservedBalance = ReservedBalance.Add(money);
        UpdatedAt = DateTime.UtcNow;
    }

    public void ReleaseReservation(decimal amount)
    {
        var money = Money.Of(amount, Currency);
        ReservedBalance = ReservedBalance.Subtract(money);
        UpdatedAt = DateTime.UtcNow;
    }

    public void CreditPnl(decimal amount)
    {
        var money = Money.Of(Math.Abs(amount), Currency);
        if (amount >= 0)
        {
            Balance = Balance.Add(money);
            _transactions.Add(Transaction.Create(Guid.NewGuid(), Id, TransactionType.PnlCredit, money, "PnL Credit"));
        }
        else
        {
            if (!Balance.IsGreaterThanOrEqual(money))
                throw new InsufficientFundsException(Balance.Amount, money.Amount);
            Balance = Balance.Subtract(money);
            _transactions.Add(Transaction.Create(Guid.NewGuid(), Id, TransactionType.PnlDebit, money, "PnL Debit"));
        }
        UpdatedAt = DateTime.UtcNow;
    }
}
