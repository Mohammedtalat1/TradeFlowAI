using TradeFlowAI.Domain.Common;
using TradeFlowAI.Domain.Enums;
using TradeFlowAI.Domain.ValueObjects;

namespace TradeFlowAI.Domain.Aggregates.Fund;

/// <summary>Transaction entity — child of Wallet aggregate.</summary>
public sealed class Transaction : Entity<Guid>
{
    public Guid WalletId { get; private set; }
    public TransactionType Type { get; private set; }
    public Money Amount { get; private set; } = null!;
    public string Reference { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; private set; }

    private Transaction() { }

    public static Transaction Create(
        Guid id, Guid walletId, TransactionType type, Money amount, string reference)
    {
        return new Transaction
        {
            Id = id,
            WalletId = walletId,
            Type = type,
            Amount = amount,
            Reference = reference,
            CreatedAt = DateTime.UtcNow
        };
    }
}
