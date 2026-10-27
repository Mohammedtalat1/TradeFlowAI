using TradeFlowAI.Domain.Common;

namespace TradeFlowAI.Domain.Aggregates.Portfolio;

/// <summary>Position entity — child of Portfolio aggregate.</summary>
public sealed class Position : Entity<Guid>
{
    public Guid PortfolioId { get; private set; }
    public string Symbol { get; private set; } = string.Empty;
    public decimal Quantity { get; private set; }
    public decimal AverageCost { get; private set; }
    public decimal CurrentPrice { get; private set; }
    public decimal MarketValue => Quantity * CurrentPrice;
    public decimal UnrealizedPnl => (CurrentPrice - AverageCost) * Quantity;
    public DateTime OpenedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    private Position() { }

    public static Position Create(Guid id, Guid portfolioId, string symbol, decimal quantity, decimal cost)
    {
        return new Position
        {
            Id = id,
            PortfolioId = portfolioId,
            Symbol = symbol,
            Quantity = quantity,
            AverageCost = cost,
            CurrentPrice = cost,
            OpenedAt = DateTime.UtcNow
        };
    }

    public void AddQuantity(decimal quantity, decimal price)
    {
        // Weighted average cost
        AverageCost = ((AverageCost * Quantity) + (price * quantity)) / (Quantity + quantity);
        Quantity += quantity;
        UpdatedAt = DateTime.UtcNow;
    }

    public decimal ReduceQuantity(decimal quantity, decimal salePrice)
    {
        if (quantity > Quantity)
            throw new InvalidOperationException("Cannot sell more than held quantity.");

        decimal pnl = (salePrice - AverageCost) * quantity;
        Quantity -= quantity;
        UpdatedAt = DateTime.UtcNow;
        return pnl;
    }

    public void UpdateMarketPrice(decimal price)
    {
        CurrentPrice = price;
        UpdatedAt = DateTime.UtcNow;
    }
}
