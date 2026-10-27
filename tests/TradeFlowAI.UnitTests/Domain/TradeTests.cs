using FluentAssertions;
using TradeFlowAI.Domain.Aggregates.Trade;
using TradeFlowAI.Domain.Enums;
using TradeFlowAI.Domain.Exceptions;

namespace TradeFlowAI.UnitTests.Domain;

public class TradeTests
{
    [Fact]
    public void Create_ValidMarketOrder_ShouldInitializePending()
    {
        // Act
        var trade = Trade.Create(
            Guid.NewGuid(), Guid.NewGuid(), "BTC/USD",
            TradeDirection.Buy, OrderType.Market, 1.5m);

        // Assert
        trade.Status.Should().Be(TradeStatus.Pending);
        trade.IsOpen.Should().BeTrue();
        trade.Quantity.Should().Be(1.5m);
        trade.FilledQuantity.Should().Be(0);
    }

    [Fact]
    public void Submit_FromPending_ShouldTransitionToSubmitted()
    {
        // Arrange
        var trade = Trade.Create(
            Guid.NewGuid(), Guid.NewGuid(), "BTC/USD",
            TradeDirection.Buy, OrderType.Market, 1.0m);

        // Act
        trade.Submit();

        // Assert
        trade.Status.Should().Be(TradeStatus.Submitted);
    }

    [Fact]
    public void Fill_WhenSubmitted_ShouldTransitionToFilledAndRaiseExecutedEvent()
    {
        // Arrange
        var trade = Trade.Create(
            Guid.NewGuid(), Guid.NewGuid(), "BTC/USD",
            TradeDirection.Buy, OrderType.Market, 1.0m);
        trade.Submit();

        // Act
        trade.Fill(68000m, 1.0m);

        // Assert
        trade.Status.Should().Be(TradeStatus.Filled);
        trade.ExecutionPrice.Should().Be(68000m);
        trade.FilledQuantity.Should().Be(1.0m);
        trade.IsOpen.Should().BeFalse();
        trade.DomainEvents.Should().HaveCount(1);
    }

    [Fact]
    public void Fill_WhenPending_ShouldThrowInvalidTradeStateException()
    {
        // Arrange
        var trade = Trade.Create(
            Guid.NewGuid(), Guid.NewGuid(), "BTC/USD",
            TradeDirection.Buy, OrderType.Market, 1.0m);

        // Act
        var act = () => trade.Fill(68000m, 1.0m);

        // Assert
        act.Should().Throw<InvalidTradeStateException>();
    }

    [Fact]
    public void Cancel_WhenSubmitted_ShouldTransitionToCancelled()
    {
        // Arrange
        var trade = Trade.Create(
            Guid.NewGuid(), Guid.NewGuid(), "BTC/USD",
            TradeDirection.Buy, OrderType.Limit, 1.0m, limitPrice: 65000m);
        trade.Submit();

        // Act
        trade.Cancel("Market moved too fast");

        // Assert
        trade.Status.Should().Be(TradeStatus.Cancelled);
        trade.CancellationReason.Should().Be("Market moved too fast");
        trade.IsOpen.Should().BeFalse();
    }
}
