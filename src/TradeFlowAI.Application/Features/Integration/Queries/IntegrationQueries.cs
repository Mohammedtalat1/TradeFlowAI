using MediatR;
using TradeFlowAI.Application.DTOs;

namespace TradeFlowAI.Application.Features.Integration.Queries;

public sealed record GetWebhooksQuery(Guid? AccountId = null) : IRequest<IReadOnlyList<WebhookDto>>;

public sealed class GetWebhooksHandler : IRequestHandler<GetWebhooksQuery, IReadOnlyList<WebhookDto>>
{
    public Task<IReadOnlyList<WebhookDto>> Handle(GetWebhooksQuery request, CancellationToken ct)
    {
        IReadOnlyList<WebhookDto> dummy =
        [
            new(Guid.NewGuid(), "https://webhook.site/trade-events", ["trade.executed", "funds.deposited"], true, DateTime.UtcNow.AddDays(-2))
        ];
        return Task.FromResult(dummy);
    }
}

public sealed record GetApiKeysQuery : IRequest<IReadOnlyList<ApiKeyDto>>;

public sealed class GetApiKeysHandler : IRequestHandler<GetApiKeysQuery, IReadOnlyList<ApiKeyDto>>
{
    public Task<IReadOnlyList<ApiKeyDto>> Handle(GetApiKeysQuery request, CancellationToken ct)
    {
        IReadOnlyList<ApiKeyDto> dummy =
        [
            new(Guid.NewGuid(), "tfai_sec", ["read", "trades.execute"], DateTime.UtcNow.AddDays(-5), DateTime.UtcNow.AddDays(360))
        ];
        return Task.FromResult(dummy);
    }
}

public sealed record GetWebhookEventsQuery : IRequest<IReadOnlyList<string>>;

public sealed class GetWebhookEventsHandler : IRequestHandler<GetWebhookEventsQuery, IReadOnlyList<string>>
{
    public Task<IReadOnlyList<string>> Handle(GetWebhookEventsQuery request, CancellationToken ct)
    {
        IReadOnlyList<string> events =
        [
            "trade.executed",
            "trade.cancelled",
            "funds.deposited",
            "funds.withdrawn",
            "risk.limit_breached",
            "strategy.signal",
            "portfolio.rebalanced"
        ];
        return Task.FromResult(events);
    }
}
