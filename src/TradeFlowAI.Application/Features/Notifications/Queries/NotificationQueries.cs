using MediatR;

namespace TradeFlowAI.Application.Features.Notifications.Queries;

public sealed record NotificationItemDto(
    Guid Id,
    string UserId,
    string Title,
    string Message,
    bool IsRead,
    DateTime CreatedAt);

public sealed record GetNotificationsQuery(string UserId) : IRequest<IReadOnlyList<NotificationItemDto>>;

public sealed class GetNotificationsHandler : IRequestHandler<GetNotificationsQuery, IReadOnlyList<NotificationItemDto>>
{
    public Task<IReadOnlyList<NotificationItemDto>> Handle(GetNotificationsQuery request, CancellationToken ct)
    {
        IReadOnlyList<NotificationItemDto> items =
        [
            new(Guid.NewGuid(), request.UserId, "Trade Executed", "BUY 0.5 BTC filled at $68,450.00", true, DateTime.UtcNow.AddHours(-1)),
            new(Guid.NewGuid(), request.UserId, "Deposit Confirmed", "Deposit of $10,000.00 USD processed successfully", false, DateTime.UtcNow.AddHours(-3))
        ];
        return Task.FromResult(items);
    }
}

public sealed record NotificationPreferencesDto(
    string UserId,
    bool EmailEnabled,
    bool WebhookEnabled,
    bool TradeAlerts,
    bool RiskAlerts);

public sealed record GetNotificationPreferencesQuery(string UserId) : IRequest<NotificationPreferencesDto>;

public sealed class GetNotificationPreferencesHandler : IRequestHandler<GetNotificationPreferencesQuery, NotificationPreferencesDto>
{
    public Task<NotificationPreferencesDto> Handle(GetNotificationPreferencesQuery request, CancellationToken ct)
    {
        return Task.FromResult(new NotificationPreferencesDto(
            request.UserId,
            true,
            true,
            true,
            true));
    }
}
