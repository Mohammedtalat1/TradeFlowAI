using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using TradeFlowAI.Application.Common.Interfaces;

namespace TradeFlowAI.Infrastructure.ExternalServices.Notifications;

public sealed class EmailService(ILogger<EmailService> logger) : IEmailService
{
    public Task SendAsync(string to, string subject, string body, CancellationToken ct = default)
    {
        logger.LogInformation("[Email Dispatched] To: {To} | Subject: {Subject}", to, subject);
        return Task.CompletedTask;
    }
}

public sealed class NotificationService(ILogger<NotificationService> logger) : INotificationService
{
    public Task NotifyAsync(string userId, string title, string message, CancellationToken ct = default)
    {
        logger.LogInformation("[In-App Alert] User: {UserId} | {Title}: {Message}", userId, title, message);
        return Task.CompletedTask;
    }
}

public sealed class WebhookDispatcher(
    ILogger<WebhookDispatcher> logger,
    IHttpClientFactory? httpClientFactory = null)
    : IWebhookDispatcher
{
    public async Task DispatchAsync(string eventType, object payload, Guid? accountId = null, CancellationToken ct = default)
    {
        var json = JsonSerializer.Serialize(new
        {
            @event = eventType,
            accountId,
            timestamp = DateTime.UtcNow,
            data = payload
        });

        logger.LogInformation("[Webhook Dispatched] Event: {Event} | Account: {AccountId} | Payload: {Payload}",
            eventType, accountId, json);

        // Simulation / non-blocking dispatch
        await Task.CompletedTask;
    }
}
