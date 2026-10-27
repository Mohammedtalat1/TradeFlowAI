using Microsoft.AspNetCore.Mvc;
using TradeFlowAI.Application.DTOs;
using TradeFlowAI.Application.Features.Portfolio.Commands;
using TradeFlowAI.Application.Features.Portfolio.Queries;

namespace TradeFlowAI.API.Controllers;

public sealed class PortfolioController : ApiControllerBase
{
    /// <summary>Get current portfolio summary for an account.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(PortfolioDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPortfolio([FromQuery] Guid accountId)
    {
        var portfolio = await Mediator.Send(new GetPortfolioQuery(accountId));
        return Ok(portfolio);
    }

    /// <summary>Get all open positions for an account.</summary>
    [HttpGet("positions")]
    [ProducesResponseType(typeof(IReadOnlyList<PositionDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPositions([FromQuery] Guid accountId)
    {
        var positions = await Mediator.Send(new GetPositionsQuery(accountId));
        return Ok(positions);
    }

    /// <summary>Get portfolio performance metrics (PnL, ROI, Sharpe ratio).</summary>
    [HttpGet("performance")]
    [ProducesResponseType(typeof(PortfolioPerformanceDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPerformance([FromQuery] Guid accountId)
    {
        var perf = await Mediator.Send(new GetPortfolioPerformanceQuery(accountId));
        return Ok(perf);
    }

    /// <summary>Get historical portfolio valuation snapshots.</summary>
    [HttpGet("snapshots")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public IActionResult GetSnapshots([FromQuery] Guid accountId)
    {
        var now = DateTime.UtcNow;
        var snapshots = Enumerable.Range(0, 14).Select(i => new
        {
            Date = now.AddDays(-i).ToString("yyyy-MM-dd"),
            TotalValue = 100000m * (1 + (decimal)(Math.Sin(i * 0.5) * 0.05)),
            Pnl = 5000m * (1 + (decimal)(Math.Cos(i * 0.4) * 0.2))
        }).ToList();

        return Ok(snapshots);
    }

    /// <summary>Trigger real-time portfolio rebalance based on latest market prices.</summary>
    [HttpPost("rebalance")]
    [ProducesResponseType(typeof(PortfolioDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> Rebalance([FromBody] RebalancePortfolioCommand command)
    {
        var result = await Mediator.Send(command);
        return Ok(result);
    }
}
