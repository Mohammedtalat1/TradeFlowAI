using TradeFlowAI.Domain.Common;
using TradeFlowAI.Domain.Enums;
using TradeFlowAI.Domain.Events;
using TradeFlowAI.Domain.Exceptions;
using TradeFlowAI.Domain.ValueObjects;

namespace TradeFlowAI.Domain.Aggregates.Account;

/// <summary>
/// Account Aggregate Root.
/// Guards all invariants related to user trading accounts.
/// </summary>
public sealed class Account : AggregateRoot<Guid>
{
    public string UserId { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public AccountStatus Status { get; private set; }
    public RiskProfile RiskProfile { get; private set; } = RiskProfile.Conservative();
    public Guid? ActiveStrategyId { get; private set; }
    public bool IsAutoTradingEnabled { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    private Account() { }

    public static Account Create(Guid id, string userId, string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        var account = new Account
        {
            Id = id,
            UserId = userId,
            Name = name,
            Status = AccountStatus.PendingKyc,
            IsAutoTradingEnabled = false,
            CreatedAt = DateTime.UtcNow
        };

        account.RaiseDomainEvent(new AccountCreatedEvent(
            Guid.NewGuid(), DateTime.UtcNow, id, userId));

        return account;
    }

    public void Activate()
    {
        if (Status != AccountStatus.PendingKyc)
            throw new DomainException("Only accounts pending KYC can be activated.");
        Status = AccountStatus.Active;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Suspend(string reason)
    {
        if (Status == AccountStatus.Closed)
            throw new DomainException("Cannot suspend a closed account.");
        Status = AccountStatus.Suspended;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Close()
    {
        Status = AccountStatus.Closed;
        IsAutoTradingEnabled = false;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateRiskProfile(RiskProfile riskProfile)
    {
        EnsureActive();
        RiskProfile = riskProfile;
        UpdatedAt = DateTime.UtcNow;
    }

    public void EnableAutoTrading(Guid strategyId)
    {
        EnsureActive();
        ActiveStrategyId = strategyId;
        IsAutoTradingEnabled = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public void DisableAutoTrading()
    {
        IsAutoTradingEnabled = false;
        ActiveStrategyId = null;
        UpdatedAt = DateTime.UtcNow;
    }

    private void EnsureActive()
    {
        if (Status != AccountStatus.Active)
            throw new DomainException($"Account is not active. Current status: {Status}");
    }
}
