using FluentValidation;
using MediatR;
using TradeFlowAI.Application.Common.Interfaces;
using TradeFlowAI.Application.DTOs;

namespace TradeFlowAI.Application.Features.Integration.Commands;

public sealed record RegisterWebhookCommand(
    string Url,
    string[] Events,
    Guid? AccountId = null
) : IRequest<WebhookDto>;

public sealed class RegisterWebhookValidator : AbstractValidator<RegisterWebhookCommand>
{
    public RegisterWebhookValidator()
    {
        RuleFor(x => x.Url).NotEmpty().Must(uri => Uri.TryCreate(uri, UriKind.Absolute, out _))
            .WithMessage("A valid absolute URL is required.");
        RuleFor(x => x.Events).NotEmpty();
    }
}

public sealed class RegisterWebhookHandler(IWebhookDispatcher dispatcher)
    : IRequestHandler<RegisterWebhookCommand, WebhookDto>
{
    public Task<WebhookDto> Handle(RegisterWebhookCommand request, CancellationToken ct)
    {
        var webhook = new WebhookDto(
            Guid.NewGuid(),
            request.Url,
            request.Events,
            true,
            DateTime.UtcNow);

        return Task.FromResult(webhook);
    }
}

public sealed record RemoveWebhookCommand(Guid Id) : IRequest<bool>;

public sealed class RemoveWebhookHandler : IRequestHandler<RemoveWebhookCommand, bool>
{
    public Task<bool> Handle(RemoveWebhookCommand request, CancellationToken ct)
    {
        return Task.FromResult(true);
    }
}

public sealed record GenerateApiKeyCommand(
    string Name,
    string[] Scopes,
    int? ValidDays = 365
) : IRequest<GeneratedApiKeyDto>;

public sealed record GeneratedApiKeyDto(
    Guid Id,
    string KeyPrefix,
    string PlainApiKey,
    string[] Scopes,
    DateTime CreatedAt,
    DateTime? ExpiresAt);

public sealed class GenerateApiKeyHandler : IRequestHandler<GenerateApiKeyCommand, GeneratedApiKeyDto>
{
    public Task<GeneratedApiKeyDto> Handle(GenerateApiKeyCommand request, CancellationToken ct)
    {
        var id = Guid.NewGuid();
        var rawToken = Convert.ToBase64String(Guid.NewGuid().ToByteArray())
            .Replace("=", "").Replace("+", "").Replace("/", "");
        var fullKey = $"tfai_{rawToken}{Guid.NewGuid():N}";
        var prefix = fullKey[..8];

        var dto = new GeneratedApiKeyDto(
            id,
            prefix,
            fullKey,
            request.Scopes,
            DateTime.UtcNow,
            request.ValidDays.HasValue ? DateTime.UtcNow.AddDays(request.ValidDays.Value) : null);

        return Task.FromResult(dto);
    }
}

public sealed record RevokeApiKeyCommand(Guid Id) : IRequest<bool>;

public sealed class RevokeApiKeyHandler : IRequestHandler<RevokeApiKeyCommand, bool>
{
    public Task<bool> Handle(RevokeApiKeyCommand request, CancellationToken ct)
    {
        return Task.FromResult(true);
    }
}

public sealed record TestWebhookCommand(string Url, string EventType) : IRequest<bool>;

public sealed class TestWebhookHandler(IWebhookDispatcher dispatcher)
    : IRequestHandler<TestWebhookCommand, bool>
{
    public async Task<bool> Handle(TestWebhookCommand request, CancellationToken ct)
    {
        await dispatcher.DispatchAsync(request.EventType, new
        {
            test = true,
            timestamp = DateTime.UtcNow,
            message = $"Test webhook dispatched for event: {request.EventType}"
        }, null, ct);

        return true;
    }
}
