using MediatR;
using TradeFlowAI.Application.Common.Interfaces;
using TradeFlowAI.Application.DTOs;

namespace TradeFlowAI.Application.Features.MarketData.Queries;

public sealed record GetAssetPriceQuery(string Symbol) : IRequest<decimal>;

public sealed class GetAssetPriceHandler(IMarketDataService marketData)
    : IRequestHandler<GetAssetPriceQuery, decimal>
{
    public async Task<decimal> Handle(GetAssetPriceQuery request, CancellationToken ct)
    {
        return await marketData.GetCurrentPriceAsync(request.Symbol, ct);
    }
}

public sealed record GetCandlesticksQuery(string Symbol, string Interval = "1h", int Limit = 50)
    : IRequest<IReadOnlyList<OhlcvBar>>;

public sealed class GetCandlesticksHandler(IMarketDataService marketData)
    : IRequestHandler<GetCandlesticksQuery, IReadOnlyList<OhlcvBar>>
{
    public async Task<IReadOnlyList<OhlcvBar>> Handle(GetCandlesticksQuery request, CancellationToken ct)
    {
        return await marketData.GetHistoricalBarsAsync(request.Symbol, request.Interval, request.Limit, ct);
    }
}

public sealed record GetMarketSummaryQuery(string Symbol) : IRequest<MarketSummaryDto>;

public sealed class GetMarketSummaryHandler(IMarketDataService marketData)
    : IRequestHandler<GetMarketSummaryQuery, MarketSummaryDto>
{
    public async Task<MarketSummaryDto> Handle(GetMarketSummaryQuery request, CancellationToken ct)
    {
        var summary = await marketData.GetMarketSummaryAsync(request.Symbol, ct);
        return new MarketSummaryDto(
            summary.Symbol,
            summary.Price,
            summary.Change24h,
            summary.ChangePercent24h,
            summary.Volume24h,
            summary.High24h,
            summary.Low24h);
    }
}

public sealed record GetSupportedAssetsQuery : IRequest<IReadOnlyList<string>>;

public sealed class GetSupportedAssetsHandler(IMarketDataService marketData)
    : IRequestHandler<GetSupportedAssetsQuery, IReadOnlyList<string>>
{
    public async Task<IReadOnlyList<string>> Handle(GetSupportedAssetsQuery request, CancellationToken ct)
    {
        return await marketData.GetSupportedSymbolsAsync(ct);
    }
}

public sealed record OrderBookEntry(decimal Price, decimal Quantity);
public sealed record OrderBookDto(string Symbol, IReadOnlyList<OrderBookEntry> Bids, IReadOnlyList<OrderBookEntry> Asks, DateTime Timestamp);

public sealed record GetOrderBookQuery(string Symbol) : IRequest<OrderBookDto>;

public sealed class GetOrderBookHandler(IMarketDataService marketData)
    : IRequestHandler<GetOrderBookQuery, OrderBookDto>
{
    public async Task<OrderBookDto> Handle(GetOrderBookQuery request, CancellationToken ct)
    {
        var price = await marketData.GetCurrentPriceAsync(request.Symbol, ct);
        var bids = new List<OrderBookEntry>
        {
            new(price * 0.999m, 1.5m),
            new(price * 0.998m, 3.2m),
            new(price * 0.997m, 5.0m),
            new(price * 0.995m, 10.0m)
        };
        var asks = new List<OrderBookEntry>
        {
            new(price * 1.001m, 1.2m),
            new(price * 1.002m, 2.8m),
            new(price * 1.003m, 4.5m),
            new(price * 1.005m, 8.0m)
        };

        return new OrderBookDto(request.Symbol, bids, asks, DateTime.UtcNow);
    }
}
