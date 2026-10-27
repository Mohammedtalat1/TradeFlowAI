using FluentAssertions;
using TradeFlowAI.Application.Common.Interfaces;
using TradeFlowAI.Domain.Enums;
using TradeFlowAI.Infrastructure.TradingStrategies;

namespace TradeFlowAI.UnitTests.Strategies;

public class StrategyAlgorithmTests
{
    [Fact]
    public void StrategyFactory_ShouldResolveRegisteredAlgorithmsCorrectly()
    {
        // Arrange
        IStrategyAlgorithm momentum = new MomentumStrategy();
        IStrategyAlgorithm meanReversion = new MeanReversionStrategy();
        IStrategyAlgorithm ai = new AiDrivenStrategy();

        var factory = new StrategyFactory([momentum, meanReversion, ai]);

        // Act & Assert
        factory.GetStrategy(StrategyType.Momentum).Should().BeOfType<MomentumStrategy>();
        factory.GetStrategy(StrategyType.MeanReversion).Should().BeOfType<MeanReversionStrategy>();
        factory.GetStrategy(StrategyType.AiDriven).Should().BeOfType<AiDrivenStrategy>();
    }

    [Fact]
    public async Task MomentumStrategy_WithBullishCrossover_ShouldGenerateBuySignal()
    {
        // Arrange
        var strategy = new MomentumStrategy();
        var bars = new List<OhlcvBar>();
        var now = DateTime.UtcNow;

        // Create 20 bars with steady uptrend so short SMA > long SMA
        for (int i = 20; i >= 1; i--)
        {
            var price = 50000m + (20 - i) * 1000m;
            bars.Add(new OhlcvBar(now.AddHours(-i), price, price + 100, price - 100, price, 1000m));
        }

        // Act
        var result = await strategy.EvaluateAsync("BTC/USD", 70000m, bars, "{}");

        // Assert
        result.Signal.Should().Be("BUY");
        result.Confidence.Should().BeGreaterThan(0.70m);
    }

    [Fact]
    public async Task MeanReversionStrategy_WhenSeverelyOversold_ShouldGenerateBuySignal()
    {
        // Arrange
        var strategy = new MeanReversionStrategy();
        var bars = new List<OhlcvBar>();
        var now = DateTime.UtcNow;

        // Normal distribution around 100
        for (int i = 20; i >= 1; i--)
        {
            bars.Add(new OhlcvBar(now.AddHours(-i), 100m, 102m, 98m, 100m, 1000m));
        }

        // Act: current price crashed to 50 (well below 2 StdDev)
        var result = await strategy.EvaluateAsync("TEST", 50m, bars, "{}");

        // Assert
        result.Signal.Should().Be("BUY");
        result.Rationale.Should().Contain("Oversold");
    }
}
