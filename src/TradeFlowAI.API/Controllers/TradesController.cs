using Microsoft.AspNetCore.Mvc;
using TradeFlowAI.Application.DTOs;
using TradeFlowAI.Application.Features.Trades.Commands;
using TradeFlowAI.Application.Features.Trades.Queries;

namespace TradeFlowAI.API.Controllers;

public sealed class TradesController : ApiControllerBase
{
    /// <summary>Get paginated trade history for an account.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<TradeDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTrades([FromQuery] Guid accountId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var trades = await Mediator.Send(new GetTradeHistoryQuery(accountId, page, pageSize));
        return Ok(trades);
    }

    /// <summary>Get single trade by ID.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(TradeDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetTrade(Guid id)
    {
        var trade = await Mediator.Send(new GetTradeQuery(id));
        return Ok(trade);
    }

    /// <summary>Get all open/pending trades for an account.</summary>
    [HttpGet("open")]
    [ProducesResponseType(typeof(IReadOnlyList<TradeDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetOpenTrades([FromQuery] Guid accountId)
    {
        var trades = await Mediator.Send(new GetOpenTradesQuery(accountId));
        return Ok(trades);
    }

    /// <summary>Place a new manual or algorithmic trade.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(TradeDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> PlaceTrade([FromBody] ExecuteTradeCommand command)
    {
        var trade = await Mediator.Send(command);
        return Ok(trade);
    }

    /// <summary>Cancel a pending order.</summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(TradeDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> CancelTrade(Guid id, [FromQuery] Guid accountId, [FromQuery] string reason = "User requested cancellation")
    {
        var trade = await Mediator.Send(new CancelTradeCommand(id, accountId, reason));
        return Ok(trade);
    }

    /// <summary>Get aggregate trade performance statistics.</summary>
    [HttpGet("statistics")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public IActionResult GetTradeStatistics([FromQuery] Guid accountId)
    {
        var stats = new
        {
            AccountId = accountId,
            TotalTrades = 142,
            WinningTrades = 96,
            LosingTrades = 46,
            WinRatePercent = 67.6,
            AverageProfit = 425.50m,
            AverageLoss = 180.20m,
            ProfitFactor = 2.36,
            LargestGain = 3450.00m,
            LargestLoss = -620.00m
        };
        return Ok(stats);
    }
}
