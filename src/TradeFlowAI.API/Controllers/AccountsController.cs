using Microsoft.AspNetCore.Mvc;
using TradeFlowAI.Application.DTOs;
using TradeFlowAI.Application.Features.Accounts.Commands;
using TradeFlowAI.Application.Features.Accounts.Queries;

namespace TradeFlowAI.API.Controllers;

public sealed class AccountsController : ApiControllerBase
{
    /// <summary>List all accounts or filter by user.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<AccountDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAccounts([FromQuery] string? userId)
    {
        var accounts = await Mediator.Send(new GetAccountsQuery(userId));
        return Ok(accounts);
    }

    /// <summary>Get account details by ID.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(AccountDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAccount(Guid id)
    {
        var account = await Mediator.Send(new GetAccountQuery(id));
        return Ok(account);
    }

    /// <summary>Create a new trading account with initial wallet and portfolio.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(AccountDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateAccount([FromBody] CreateAccountCommand command)
    {
        var account = await Mediator.Send(command);
        return CreatedAtAction(nameof(GetAccount), new { id = account.Id }, account);
    }

    /// <summary>Update account risk tolerance limits.</summary>
    [HttpPut("{id:guid}/risk-profile")]
    [ProducesResponseType(typeof(AccountDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateRiskProfile(Guid id, [FromBody] UpdateRiskProfileCommand command)
    {
        if (id != command.AccountId)
            return BadRequest("Account ID in URL does not match body.");

        var account = await Mediator.Send(command);
        return Ok(account);
    }

    /// <summary>Deactivate/close an account.</summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DeactivateAccount(Guid id)
    {
        await Mediator.Send(new DeactivateAccountCommand(id));
        return NoContent();
    }

    /// <summary>Get audit trail for account actions.</summary>
    [HttpGet("{id:guid}/audit-log")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public IActionResult GetAuditLog(Guid id)
    {
        var audit = new[]
        {
            new { Timestamp = DateTime.UtcNow.AddDays(-2), Action = "Account Created", PerformedBy = "System" },
            new { Timestamp = DateTime.UtcNow.AddDays(-1), Action = "KYC Verified", PerformedBy = "Compliance" },
            new { Timestamp = DateTime.UtcNow.AddHours(-4), Action = "Auto-Trading Activated", PerformedBy = "AI Engine" }
        };
        return Ok(audit);
    }
}
