using Microsoft.AspNetCore.Mvc;
using TradeFlowAI.Application.DTOs;
using TradeFlowAI.Application.Features.Funds.Commands;
using TradeFlowAI.Application.Features.Funds.Queries;

namespace TradeFlowAI.API.Controllers;

public sealed class FundsController : ApiControllerBase
{
    /// <summary>Get current wallet balance for an account.</summary>
    [HttpGet("balance")]
    [ProducesResponseType(typeof(WalletDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetBalance([FromQuery] Guid accountId)
    {
        var balance = await Mediator.Send(new GetWalletBalanceQuery(accountId));
        return Ok(balance);
    }

    /// <summary>Deposit funds into account wallet.</summary>
    [HttpPost("deposit")]
    [ProducesResponseType(typeof(WalletDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Deposit([FromBody] DepositFundsCommand command)
    {
        var wallet = await Mediator.Send(command);
        return Ok(wallet);
    }

    /// <summary>Request funds withdrawal from account wallet.</summary>
    [HttpPost("withdraw")]
    [ProducesResponseType(typeof(WalletDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Withdraw([FromBody] WithdrawFundsCommand command)
    {
        var wallet = await Mediator.Send(command);
        return Ok(wallet);
    }

    /// <summary>Get paginated transaction history for an account.</summary>
    [HttpGet("transactions")]
    [ProducesResponseType(typeof(PagedResult<TransactionDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTransactions([FromQuery] Guid accountId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var history = await Mediator.Send(new GetTransactionHistoryQuery(accountId, page, pageSize));
        return Ok(history);
    }

    /// <summary>Get single transaction detail.</summary>
    [HttpGet("transactions/{id:guid}")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public IActionResult GetTransaction(Guid id)
    {
        return Ok(new
        {
            Id = id,
            Status = "Completed",
            Timestamp = DateTime.UtcNow,
            Method = "Bank Wire"
        });
    }
}
