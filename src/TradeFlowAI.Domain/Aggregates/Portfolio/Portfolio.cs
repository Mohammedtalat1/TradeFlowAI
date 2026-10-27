using TradeFlowAI.Domain.Common;
using TradeFlowAI.Domain.Enums;
using TradeFlowAI.Domain.Events;

namespace TradeFlowAI.Domain.Aggregates.Portfolio;

/// <summary>Portfolio Aggregate Root — tracks positions and P&L.</summary>
public sealed class Portfolio : AggregateRoot<Guid>
{
    private readonly List<Position> _positions = [];

    public Guid AccountId { get; private set; }
    public decimal TotalValue { get; private set; }
    public decimal TotalCost { get; private set; }
    public decimal UnrealizedPnl => TotalValue - TotalCost;
    public decimal RealizedPnl { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    public IReadOnlyList<Position> Positions => _positions.AsReadOnly();

    private Portfolio() { }

    public static Portfolio Create(Guid id, Guid accountId)
    {
        return new Portfolio
        {
            Id = id,
            AccountId = accountId,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void ApplyTradeFill(string symbol, decimal quantity, decimal price, TradeDirection direction)
    {
        var existing = _positions.FirstOrDefault(p => p.Symbol == symbol);

        if (direction == TradeDirection.Buy)
        {
            if (existing is null)
            {
                var position = Position.Create(Guid.NewGuid(), Id, symbol, quantity, price);
                _positions.Add(position);
            }
            else
            {
                existing.AddQuantity(quantity, price);
            }
            TotalCost += quantity * price;
        }
        else // Sell
        {
            if (existing is null)
                throw new InvalidOperationException($"No open position for {symbol} to sell.");

            decimal pnl = existing.ReduceQuantity(quantity, price);
            RealizedPnl += pnl;
            TotalCost -= quantity * existing.AverageCost;

            if (existing.Quantity <= 0)
                _positions.Remove(existing);
        }

        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateMarketValues(Dictionary<string, decimal> prices)
    {
        TotalValue = 0;
        foreach (var position in _positions)
        {
            if (prices.TryGetValue(position.Symbol, out var price))
            {
                position.UpdateMarketPrice(price);
                TotalValue += position.MarketValue;
            }
        }
        UpdatedAt = DateTime.UtcNow;

        RaiseDomainEvent(new PortfolioRebalancedEvent(
            Guid.NewGuid(), DateTime.UtcNow, Id, AccountId));
    }
}
