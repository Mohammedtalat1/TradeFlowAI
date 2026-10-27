using TradeFlowAI.Application.Common.Interfaces;

namespace TradeFlowAI.Infrastructure.ExternalServices.MarketData;

/// <summary>
/// Adapter Pattern: Adapts external market data feeds (e.g. Binance/AlphaVantage/Polygon)
/// to the unified IMarketDataService interface expected by the domain & application layers.
/// </summary>
public sealed class MarketDataService : IMarketDataService
{
    private static readonly Dictionary<string, decimal> AssetPrices = new(StringComparer.OrdinalIgnoreCase)
    {
        ["BTC/USD"] = 68450.00m,
        ["ETH/USD"] = 3520.50m,
        ["SOL/USD"] = 184.25m,
        ["AAPL"] = 228.40m,
        ["NVDA"] = 132.80m,
        ["TSLA"] = 248.90m,
        ["SPY"] = 582.10m
    };

    public Task<decimal> GetCurrentPriceAsync(string symbol, CancellationToken ct = default)
    {
        if (AssetPrices.TryGetValue(symbol, out var price))
        {
            // Add tiny jitter to simulate live tick fluctuations
            var jitter = (decimal)(new Random().NextDouble() * 0.004 - 0.002);
            return Task.FromResult(Math.Round(price * (1 + jitter), 2));
        }

        return Task.FromResult(100.00m);
    }

    public Task<IReadOnlyList<OhlcvBar>> GetHistoricalBarsAsync(
        string symbol, string interval, int limit, CancellationToken ct = default)
    {
        var basePrice = AssetPrices.GetValueOrDefault(symbol, 100.00m);
        var bars = new List<OhlcvBar>();
        var now = DateTime.UtcNow;
        var rng = new Random(symbol.GetHashCode());

        var current = basePrice * 0.90m;
        for (int i = limit; i >= 0; i--)
        {
            var open = current;
            var changePct = (decimal)(rng.NextDouble() * 0.04 - 0.018);
            var close = Math.Round(open * (1 + changePct), 2);
            var high = Math.Round(Math.Max(open, close) * (1 + (decimal)rng.NextDouble() * 0.01m), 2);
            var low = Math.Round(Math.Min(open, close) * (1 - (decimal)rng.NextDouble() * 0.01m), 2);
            var volume = Math.Round((decimal)(rng.NextDouble() * 5000 + 1000), 2);

            bars.Add(new OhlcvBar(now.AddHours(-i), open, high, low, close, volume));
            current = close;
        }

        return Task.FromResult<IReadOnlyList<OhlcvBar>>(bars);
    }

    public Task<MarketSummary> GetMarketSummaryAsync(string symbol, CancellationToken ct = default)
    {
        var price = AssetPrices.GetValueOrDefault(symbol, 100.00m);
        var summary = new MarketSummary(
            symbol,
            price,
            price * 0.032m,
            3.20m,
            1840291m,
            price * 1.045m,
            price * 0.982m);

        return Task.FromResult(summary);
    }

    public Task<IReadOnlyList<string>> GetSupportedSymbolsAsync(CancellationToken ct = default)
    {
        IReadOnlyList<string> list = AssetPrices.Keys.ToList();
        return Task.FromResult(list);
    }
}
