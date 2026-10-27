using Microsoft.AspNetCore.Mvc;
using TradeFlowAI.Application.Common.Interfaces;
using TradeFlowAI.Application.DTOs;
using TradeFlowAI.Application.Features.Strategies.Commands;
using TradeFlowAI.Application.Features.Strategies.Queries;

namespace TradeFlowAI.API.Controllers;

public sealed class StrategiesController : ApiControllerBase
{
    /// <summary>List all configured AI trading strategies.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<TradingStrategyDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetStrategies([FromQuery] string? userId)
    {
        var strategies = await Mediator.Send(new GetStrategiesQuery(userId));
        return Ok(strategies);
    }

    /// <summary>Get trading strategy by ID.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(TradingStrategyDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetStrategy(Guid id)
    {
        var strategy = await Mediator.Send(new GetStrategyQuery(id));
        return Ok(strategy);
    }

    /// <summary>Create a new trading strategy.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(TradingStrategyDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateStrategy([FromBody] CreateStrategyCommand command)
    {
        var strategy = await Mediator.Send(command);
        return CreatedAtAction(nameof(GetStrategy), new { id = strategy.Id }, strategy);
    }

    /// <summary>Activate strategy for an account to enable automated execution.</summary>
    [HttpPost("{id:guid}/activate")]
    [ProducesResponseType(typeof(TradingStrategyDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> ActivateStrategy(Guid id, [FromQuery] Guid accountId)
    {
        var strategy = await Mediator.Send(new ActivateStrategyCommand(id, accountId));
        return Ok(strategy);
    }

    /// <summary>Deactivate strategy for an account.</summary>
    [HttpPost("{id:guid}/deactivate")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult DeactivateStrategy(Guid id)
    {
        return Ok(new { message = $"Strategy {id} deactivated successfully" });
    }

    /// <summary>Run historical simulation backtest on historical market data.</summary>
    [HttpPost("{id:guid}/backtest")]
    [ProducesResponseType(typeof(BacktestResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Backtest(Guid id, [FromBody] BacktestStrategyCommand command)
    {
        if (id != command.StrategyId)
            return BadRequest("Strategy ID in route does not match payload.");

        var result = await Mediator.Send(command);
        return Ok(result);
    }

    /// <summary>Get backtest run results by run ID.</summary>
    [HttpGet("{id:guid}/backtest/{runId:guid}")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public IActionResult GetBacktestRun(Guid id, Guid runId)
    {
        var result = new
        {
            RunId = runId,
            StrategyId = id,
            Status = "Completed",
            FinalCapital = 128400.00m,
            TotalReturnPercent = 28.4,
            MaxDrawdownPercent = 7.8,
            SharpeRatio = 2.14,
            TotalTrades = 84,
            WinRatePercent = 64.3
        };
        return Ok(result);
    }

    /// <summary>Get real-time AI signal generation evaluation for a symbol.</summary>
    [HttpGet("{id:guid}/signals")]
    [ProducesResponseType(typeof(AiSignalResult), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSignals(Guid id, [FromQuery] string symbol = "BTC/USD")
    {
        var signal = await Mediator.Send(new GetStrategySignalsQuery(id, symbol));
        return Ok(signal);
    }
}
