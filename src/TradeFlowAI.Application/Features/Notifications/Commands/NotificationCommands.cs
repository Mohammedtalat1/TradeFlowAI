using FluentValidation;
using MediatR;
using TradeFlowAI.Application.Common.Interfaces;

namespace TradeFlowAI.Application.Features.Notifications.Commands;

public sealed record SendNotificationCommand(
    string UserId,
    string Title,
    string Message
) : IRequest<bool>;

public sealed class SendNotificationValidator : AbstractValidator<SendNotificationCommand>
{
    public SendNotificationValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Message).NotEmpty().MaximumLength(2000);
    }
}

public sealed class SendNotificationHandler(INotificationService notificationService)
    : IRequestHandler<SendNotificationCommand, bool>
{
    public async Task<bool> Handle(SendNotificationCommand request, CancellationToken ct)
    {
        await notificationService.NotifyAsync(request.UserId, request.Title, request.Message, ct);
        return true;
    }
}

public sealed record MarkNotificationReadCommand(Guid Id) : IRequest<bool>;

public sealed class MarkNotificationReadHandler : IRequestHandler<MarkNotificationReadCommand, bool>
{
    public Task<bool> Handle(MarkNotificationReadCommand request, CancellationToken ct) => Task.FromResult(true);
}

public sealed record DeleteNotificationCommand(Guid Id) : IRequest<bool>;

public sealed class DeleteNotificationHandler : IRequestHandler<DeleteNotificationCommand, bool>
{
    public Task<bool> Handle(DeleteNotificationCommand request, CancellationToken ct) => Task.FromResult(true);
}

public sealed record UpdateNotificationPreferencesCommand(
    string UserId,
    bool EmailEnabled,
    bool WebhookEnabled,
    bool TradeAlerts,
    bool RiskAlerts
) : IRequest<bool>;

public sealed class UpdateNotificationPreferencesHandler : IRequestHandler<UpdateNotificationPreferencesCommand, bool>
{
    public Task<bool> Handle(UpdateNotificationPreferencesCommand request, CancellationToken ct) => Task.FromResult(true);
}
