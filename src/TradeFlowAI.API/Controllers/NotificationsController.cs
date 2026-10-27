using Microsoft.AspNetCore.Mvc;
using TradeFlowAI.Application.Features.Notifications.Commands;
using TradeFlowAI.Application.Features.Notifications.Queries;

namespace TradeFlowAI.API.Controllers;

public sealed class NotificationsController : ApiControllerBase
{
    /// <summary>Get user notification list.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<NotificationItemDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetNotifications([FromQuery] string userId = "default-user")
    {
        var notifications = await Mediator.Send(new GetNotificationsQuery(userId));
        return Ok(notifications);
    }

    /// <summary>Send an alert notification to a user.</summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SendNotification([FromBody] SendNotificationCommand command)
    {
        await Mediator.Send(command);
        return Ok(new { message = "Notification queued for delivery" });
    }

    /// <summary>Mark a notification as read.</summary>
    [HttpPut("{id:guid}/read")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> MarkRead(Guid id)
    {
        await Mediator.Send(new MarkNotificationReadCommand(id));
        return Ok(new { message = "Marked as read" });
    }

    /// <summary>Delete a notification.</summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete(Guid id)
    {
        await Mediator.Send(new DeleteNotificationCommand(id));
        return NoContent();
    }

    /// <summary>Get user notification preferences.</summary>
    [HttpGet("preferences")]
    [ProducesResponseType(typeof(NotificationPreferencesDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPreferences([FromQuery] string userId = "default-user")
    {
        var prefs = await Mediator.Send(new GetNotificationPreferencesQuery(userId));
        return Ok(prefs);
    }

    /// <summary>Update user notification channel preferences.</summary>
    [HttpPut("preferences")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdatePreferences([FromBody] UpdateNotificationPreferencesCommand command)
    {
        await Mediator.Send(command);
        return Ok(new { message = "Notification preferences updated" });
    }
}
