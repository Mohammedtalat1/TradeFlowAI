using FluentAssertions;
using TradeFlowAI.Domain.Aggregates.Portfolio;
using TradeFlowAI.Domain.Enums;

namespace TradeFlowAI.UnitTests.Domain;

public class PortfolioTests
{
    [Fact]
    public void ApplyTradeFill_Buy_ShouldCreatePositionAndTrackCost()
    {
        // Arrange
        var portfolio = Portfolio.Create(Guid.NewGuid(), Guid.NewGuid());

        // Act
        portfolio.ApplyTradeFill("BTC/USD", 1.0m, 60000m, TradeDirection.Buy);

        // Assert
        portfolio.Positions.Should().HaveCount(1);
        var position = portfolio.Positions[0];
        position.Symbol.Should().Be("BTC/USD");
        position.Quantity.Should().Be(1.0m);
        position.AverageCost.Should().Be(60000m);
        portfolio.TotalCost.Should().Be(60000m);
    }

    [Fact]
    public void ApplyTradeFill_SellWithProfit_ShouldCalculateRealizedPnlAndReducePosition()
    {
        // Arrange
        var portfolio = Portfolio.Create(Guid.NewGuid(), Guid.NewGuid());
        portfolio.ApplyTradeFill("BTC/USD", 2.0m, 50000m, TradeDirection.Buy);

        // Act - Sell 1 BTC at 60,000 (Profit of 10,000)
        portfolio.ApplyTradeFill("BTC/USD", 1.0m, 60000m, TradeDirection.Sell);

        // Assert
        portfolio.Positions.Should().HaveCount(1);
        portfolio.Positions[0].Quantity.Should().Be(1.0m);
        portfolio.RealizedPnl.Should().Be(10000m);
    }

    [Fact]
    public void UpdateMarketValues_ShouldUpdatePositionAndTotalPortfolioValue()
    {
        // Arrange
        var portfolio = Portfolio.Create(Guid.NewGuid(), Guid.NewGuid());
        portfolio.ApplyTradeFill("BTC/USD", 1.0m, 60000m, TradeDirection.Buy);

        // Act
        portfolio.UpdateMarketValues(new Dictionary<string, decimal> { ["BTC/USD"] = 70000m });

        // Assert
        portfolio.TotalValue.Should().Be(70000m);
        portfolio.UnrealizedPnl.Should().Be(10000m);
        portfolio.DomainEvents.Should().HaveCount(1);
    }
}
