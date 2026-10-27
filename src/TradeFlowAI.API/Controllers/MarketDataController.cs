using Microsoft.AspNetCore.Mvc;
using TradeFlowAI.Application.Common.Interfaces;
using TradeFlowAI.Application.DTOs;
using TradeFlowAI.Application.Features.MarketData.Queries;

namespace TradeFlowAI.API.Controllers;

public sealed class MarketDataController : ApiControllerBase
{
    /// <summary>List all supported tradable assets and symbols.</summary>
    [HttpGet("assets")]
    [ProducesResponseType(typeof(IReadOnlyList<string>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAssets()
    {
        var assets = await Mediator.Send(new GetSupportedAssetsQuery());
        return Ok(assets);
    }

    /// <summary>Get current live market price for an asset symbol.</summary>
    [HttpGet("assets/{symbol}/price")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPrice(string symbol)
    {
        var decodedSymbol = Uri.UnescapeDataString(symbol);
        var price = await Mediator.Send(new GetAssetPriceQuery(decodedSymbol));
        return Ok(new { Symbol = decodedSymbol, Price = price, Timestamp = DateTime.UtcNow });
    }

    /// <summary>Get OHLCV candlestick historical bar data.</summary>
    [HttpGet("assets/{symbol}/candles")]
    [ProducesResponseType(typeof(IReadOnlyList<OhlcvBar>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCandles(string symbol, [FromQuery] string interval = "1h", [FromQuery] int limit = 50)
    {
        var decodedSymbol = Uri.UnescapeDataString(symbol);
        var bars = await Mediator.Send(new GetCandlesticksQuery(decodedSymbol, interval, limit));
        return Ok(bars);
    }

    /// <summary>Get 24-hour market summary and volatility statistics.</summary>
    [HttpGet("assets/{symbol}/summary")]
    [ProducesResponseType(typeof(MarketSummaryDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSummary(string symbol)
    {
        var decodedSymbol = Uri.UnescapeDataString(symbol);
        var summary = await Mediator.Send(new GetMarketSummaryQuery(decodedSymbol));
        return Ok(summary);
    }

    /// <summary>Get live L2 order book snapshot (bids and asks).</summary>
    [HttpGet("assets/{symbol}/orderbook")]
    [ProducesResponseType(typeof(OrderBookDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetOrderBook(string symbol)
    {
        var decodedSymbol = Uri.UnescapeDataString(symbol);
        var orderBook = await Mediator.Send(new GetOrderBookQuery(decodedSymbol));
        return Ok(orderBook);
    }
}
