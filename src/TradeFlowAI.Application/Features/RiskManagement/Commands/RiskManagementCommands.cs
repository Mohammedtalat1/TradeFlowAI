using FluentValidation;
using MediatR;
using TradeFlowAI.Application.Common.Exceptions;
using TradeFlowAI.Application.Features.RiskManagement.Queries;
using TradeFlowAI.Domain.Aggregates.Account;
using TradeFlowAI.Domain.Interfaces;
using TradeFlowAI.Domain.ValueObjects;

namespace TradeFlowAI.Application.Features.RiskManagement.Commands;

public sealed record ResetCircuitBreakerCommand(Guid AccountId) : IRequest<CircuitBreakerStatusDto>;

public sealed class ResetCircuitBreakerHandler(IUnitOfWork uow)
    : IRequestHandler<ResetCircuitBreakerCommand, CircuitBreakerStatusDto>
{
    public async Task<CircuitBreakerStatusDto> Handle(ResetCircuitBreakerCommand request, CancellationToken ct)
    {
        var account = await uow.Accounts.GetByIdAsync(request.AccountId, ct)
            ?? throw new NotFoundException(nameof(Account), request.AccountId);

        // Reset account to Active if it was suspended
        if (account.Status == Domain.Enums.AccountStatus.Suspended)
        {
            account.Activate();
            uow.Accounts.Update(account);
            await uow.SaveChangesAsync(ct);
        }

        return new CircuitBreakerStatusDto(account.Id, false, "Reset by administrator", null);
    }
}

public sealed record UpdateRiskLimitsCommand(
    Guid AccountId,
    decimal MaxDrawdownPercent,
    decimal MaxPositionSizePercent,
    decimal DailyLossLimitPercent,
    int MaxOpenTrades
) : IRequest<RiskLimitsDto>;

public sealed class UpdateRiskLimitsValidator : AbstractValidator<UpdateRiskLimitsCommand>
{
    public UpdateRiskLimitsValidator()
    {
        RuleFor(x => x.MaxDrawdownPercent).InclusiveBetween(1, 100);
        RuleFor(x => x.MaxPositionSizePercent).InclusiveBetween(1, 100);
        RuleFor(x => x.DailyLossLimitPercent).InclusiveBetween(1, 100);
        RuleFor(x => x.MaxOpenTrades).GreaterThan(0);
    }
}

public sealed class UpdateRiskLimitsHandler(IUnitOfWork uow)
    : IRequestHandler<UpdateRiskLimitsCommand, RiskLimitsDto>
{
    public async Task<RiskLimitsDto> Handle(UpdateRiskLimitsCommand request, CancellationToken ct)
    {
        var account = await uow.Accounts.GetByIdAsync(request.AccountId, ct)
            ?? throw new NotFoundException(nameof(Account), request.AccountId);

        var newProfile = RiskProfile.Create(
            request.MaxDrawdownPercent,
            request.MaxPositionSizePercent,
            request.DailyLossLimitPercent,
            request.MaxOpenTrades);

        account.UpdateRiskProfile(newProfile);
        uow.Accounts.Update(account);
        await uow.SaveChangesAsync(ct);

        return new RiskLimitsDto(
            account.Id,
            newProfile.MaxDrawdownPercent,
            newProfile.MaxPositionSizePercent,
            newProfile.DailyLossLimitPercent,
            newProfile.MaxOpenTrades);
    }
}
