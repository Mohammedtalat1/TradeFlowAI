using Microsoft.AspNetCore.Mvc;
using TradeFlowAI.Application.DTOs;
using TradeFlowAI.Application.Features.RiskManagement.Commands;
using TradeFlowAI.Application.Features.RiskManagement.Queries;

namespace TradeFlowAI.API.Controllers;

public sealed class RiskManagementController : ApiControllerBase
{
    /// <summary>Get real-time risk exposure metrics for an account.</summary>
    [HttpGet("metrics")]
    [ProducesResponseType(typeof(RiskMetricsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMetrics([FromQuery] Guid accountId)
    {
        var metrics = await Mediator.Send(new GetRiskMetricsQuery(accountId));
        return Ok(metrics);
    }

    /// <summary>Check circuit breaker safety status for an account.</summary>
    [HttpGet("circuit-breakers")]
    [ProducesResponseType(typeof(CircuitBreakerStatusDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCircuitBreakers([FromQuery] Guid accountId)
    {
        var status = await Mediator.Send(new GetCircuitBreakerStatusQuery(accountId));
        return Ok(status);
    }

    /// <summary>Reset tripped circuit breaker (Admin or risk officer).</summary>
    [HttpPost("circuit-breakers/{id:guid}/reset")]
    [ProducesResponseType(typeof(CircuitBreakerStatusDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> ResetCircuitBreaker(Guid id)
    {
        var status = await Mediator.Send(new ResetCircuitBreakerCommand(id));
        return Ok(status);
    }

    /// <summary>Get configured risk limits for an account.</summary>
    [HttpGet("limits")]
    [ProducesResponseType(typeof(RiskLimitsDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetLimits([FromQuery] Guid accountId)
    {
        var limits = await Mediator.Send(new GetRiskLimitsQuery(accountId));
        return Ok(limits);
    }

    /// <summary>Update account risk limits (Max drawdown, leverage, position sizes).</summary>
    [HttpPut("limits")]
    [ProducesResponseType(typeof(RiskLimitsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateLimits([FromBody] UpdateRiskLimitsCommand command)
    {
        var updated = await Mediator.Send(command);
        return Ok(updated);
    }
}
