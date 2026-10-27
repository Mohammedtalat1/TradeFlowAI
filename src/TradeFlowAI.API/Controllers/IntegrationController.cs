using Microsoft.AspNetCore.Mvc;
using TradeFlowAI.Application.DTOs;
using TradeFlowAI.Application.Features.Integration.Commands;
using TradeFlowAI.Application.Features.Integration.Queries;

namespace TradeFlowAI.API.Controllers;

public sealed class IntegrationController : ApiControllerBase
{
    /// <summary>List all registered external webhooks.</summary>
    [HttpGet("webhooks")]
    [ProducesResponseType(typeof(IReadOnlyList<WebhookDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetWebhooks([FromQuery] Guid? accountId)
    {
        var webhooks = await Mediator.Send(new GetWebhooksQuery(accountId));
        return Ok(webhooks);
    }

    /// <summary>Register a new webhook endpoint for receiving trading platform events.</summary>
    [HttpPost("webhooks")]
    [ProducesResponseType(typeof(WebhookDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RegisterWebhook([FromBody] RegisterWebhookCommand command)
    {
        var webhook = await Mediator.Send(command);
        return CreatedAtAction(nameof(GetWebhooks), new { accountId = command.AccountId }, webhook);
    }

    /// <summary>Delete a registered webhook.</summary>
    [HttpDelete("webhooks/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> RemoveWebhook(Guid id)
    {
        await Mediator.Send(new RemoveWebhookCommand(id));
        return NoContent();
    }

    /// <summary>List active API keys for machine-to-machine integrations.</summary>
    [HttpGet("api-keys")]
    [ProducesResponseType(typeof(IReadOnlyList<ApiKeyDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetApiKeys()
    {
        var keys = await Mediator.Send(new GetApiKeysQuery());
        return Ok(keys);
    }

    /// <summary>Generate a new secure API Key for external trading bots or platforms.</summary>
    [HttpPost("api-keys")]
    [ProducesResponseType(typeof(GeneratedApiKeyDto), StatusCodes.Status201Created)]
    public async Task<IActionResult> GenerateApiKey([FromBody] GenerateApiKeyCommand command)
    {
        var key = await Mediator.Send(command);
        return Created("", key);
    }

    /// <summary>Revoke an API key.</summary>
    [HttpDelete("api-keys/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> RevokeApiKey(Guid id)
    {
        await Mediator.Send(new RevokeApiKeyCommand(id));
        return NoContent();
    }

    /// <summary>List all supported webhook event types.</summary>
    [HttpGet("events")]
    [ProducesResponseType(typeof(IReadOnlyList<string>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSupportedEvents()
    {
        var events = await Mediator.Send(new GetWebhookEventsQuery());
        return Ok(events);
    }

    /// <summary>Send a test webhook event to verify external receiver endpoint.</summary>
    [HttpPost("test-webhook")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> TestWebhook([FromBody] TestWebhookCommand command)
    {
        await Mediator.Send(command);
        return Ok(new { message = $"Test event '{command.EventType}' dispatched successfully" });
    }
}
