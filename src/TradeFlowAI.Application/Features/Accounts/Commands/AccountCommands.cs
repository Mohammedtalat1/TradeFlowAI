using AutoMapper;
using FluentValidation;
using MediatR;
using TradeFlowAI.Application.DTOs;
using TradeFlowAI.Domain.Aggregates.Account;
using TradeFlowAI.Domain.Aggregates.Fund;
using TradeFlowAI.Domain.Aggregates.Portfolio;
using TradeFlowAI.Domain.Interfaces;
using TradeFlowAI.Domain.ValueObjects;

namespace TradeFlowAI.Application.Features.Accounts.Commands;

// ── Create Account ─────────────────────────────────────────────────────────

public sealed record CreateAccountCommand(
    string UserId,
    string Name,
    string RiskTier   // "Conservative" | "Moderate" | "Aggressive"
) : IRequest<AccountDto>;

public sealed class CreateAccountValidator : AbstractValidator<CreateAccountCommand>
{
    public CreateAccountValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.RiskTier).Must(t => t is "Conservative" or "Moderate" or "Aggressive")
            .WithMessage("RiskTier must be Conservative, Moderate, or Aggressive.");
    }
}

public sealed class CreateAccountHandler(IUnitOfWork uow, IMapper mapper)
    : IRequestHandler<CreateAccountCommand, AccountDto>
{
    public async Task<AccountDto> Handle(CreateAccountCommand request, CancellationToken ct)
    {
        var accountId = Guid.NewGuid();

        var riskProfile = request.RiskTier switch
        {
            "Moderate" => RiskProfile.Moderate(),
            "Aggressive" => RiskProfile.Aggressive(),
            _ => RiskProfile.Conservative()
        };

        var account = Account.Create(accountId, request.UserId, request.Name);
        account.UpdateRiskProfile(riskProfile);
        account.Activate(); // Auto-activate for demo (real: require KYC)

        var wallet = Wallet.Create(Guid.NewGuid(), accountId);
        var portfolio = Portfolio.Create(Guid.NewGuid(), accountId);

        await uow.Accounts.AddAsync(account, ct);
        await uow.Wallets.AddAsync(wallet, ct);
        await uow.Portfolios.AddAsync(portfolio, ct);
        await uow.SaveChangesAsync(ct);

        return mapper.Map<AccountDto>(account);
    }
}

// ── Update Risk Profile ────────────────────────────────────────────────────

public sealed record UpdateRiskProfileCommand(
    Guid AccountId,
    decimal MaxDrawdownPercent,
    decimal MaxPositionSizePercent,
    decimal DailyLossLimitPercent,
    int MaxOpenTrades,
    decimal LeverageMultiplier
) : IRequest<AccountDto>;

public sealed class UpdateRiskProfileValidator : AbstractValidator<UpdateRiskProfileCommand>
{
    public UpdateRiskProfileValidator()
    {
        RuleFor(x => x.AccountId).NotEmpty();
        RuleFor(x => x.MaxDrawdownPercent).InclusiveBetween(1, 100);
        RuleFor(x => x.MaxPositionSizePercent).InclusiveBetween(1, 100);
        RuleFor(x => x.DailyLossLimitPercent).InclusiveBetween(1, 100);
        RuleFor(x => x.MaxOpenTrades).GreaterThan(0).LessThanOrEqualTo(100);
        RuleFor(x => x.LeverageMultiplier).GreaterThanOrEqualTo(1m);
    }
}

public sealed class UpdateRiskProfileHandler(IUnitOfWork uow, IMapper mapper)
    : IRequestHandler<UpdateRiskProfileCommand, AccountDto>
{
    public async Task<AccountDto> Handle(UpdateRiskProfileCommand request, CancellationToken ct)
    {
        var account = await uow.Accounts.GetByIdAsync(request.AccountId, ct)
            ?? throw new Application.Common.Exceptions.NotFoundException(nameof(Account), request.AccountId);

        var riskProfile = new RiskProfile(
            request.MaxDrawdownPercent,
            request.MaxPositionSizePercent,
            request.DailyLossLimitPercent,
            request.MaxOpenTrades,
            request.LeverageMultiplier);

        account.UpdateRiskProfile(riskProfile);
        uow.Accounts.Update(account);
        await uow.SaveChangesAsync(ct);

        return mapper.Map<AccountDto>(account);
    }
}
