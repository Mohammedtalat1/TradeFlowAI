using AutoMapper;
using FluentValidation;
using MediatR;
using TradeFlowAI.Application.Common.Exceptions;
using TradeFlowAI.Application.Common.Interfaces;
using TradeFlowAI.Application.DTOs;
using TradeFlowAI.Domain.Aggregates.Strategy;
using TradeFlowAI.Domain.Enums;
using TradeFlowAI.Domain.Interfaces;

namespace TradeFlowAI.Application.Features.Strategies.Commands;

// ── Create Strategy ────────────────────────────────────────────────────────

public sealed record CreateStrategyCommand(
    string Name,
    string Description,
    StrategyType Type,
    string[] SupportedSymbols,
    string CreatedByUserId,
    string ParametersJson = "{}"
) : IRequest<TradingStrategyDto>;

public sealed class CreateStrategyValidator : AbstractValidator<CreateStrategyCommand>
{
    public CreateStrategyValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.SupportedSymbols).NotEmpty();
        RuleFor(x => x.CreatedByUserId).NotEmpty();
    }
}

public sealed class CreateStrategyHandler(IUnitOfWork uow, IMapper mapper)
    : IRequestHandler<CreateStrategyCommand, TradingStrategyDto>
{
    public async Task<TradingStrategyDto> Handle(CreateStrategyCommand request, CancellationToken ct)
    {
        var strategy = TradingStrategy.Create(
            Guid.NewGuid(), request.Name, request.Description,
            request.Type, request.SupportedSymbols,
            request.CreatedByUserId, request.ParametersJson);

        await uow.Strategies.AddAsync(strategy, ct);
        await uow.SaveChangesAsync(ct);

        return mapper.Map<TradingStrategyDto>(strategy);
    }
}

// ── Activate / Deactivate Strategy ────────────────────────────────────────

public sealed record ActivateStrategyCommand(Guid StrategyId, Guid AccountId) : IRequest<TradingStrategyDto>;

public sealed class ActivateStrategyHandler(IUnitOfWork uow, IMapper mapper)
    : IRequestHandler<ActivateStrategyCommand, TradingStrategyDto>
{
    public async Task<TradingStrategyDto> Handle(ActivateStrategyCommand request, CancellationToken ct)
    {
        var strategy = await uow.Strategies.GetByIdAsync(request.StrategyId, ct)
            ?? throw new NotFoundException(nameof(TradingStrategy), request.StrategyId);

        var account = await uow.Accounts.GetByIdAsync(request.AccountId, ct)
            ?? throw new NotFoundException(nameof(Domain.Aggregates.Account.Account), request.AccountId);

        strategy.Activate();
        account.EnableAutoTrading(strategy.Id);

        uow.Strategies.Update(strategy);
        uow.Accounts.Update(account);
        await uow.SaveChangesAsync(ct);

        return mapper.Map<TradingStrategyDto>(strategy);
    }
}

// ── Backtest Strategy ──────────────────────────────────────────────────────

public sealed record BacktestStrategyCommand(
    Guid StrategyId,
    string Symbol,
    DateTime From,
    DateTime To,
    decimal InitialCapital
) : IRequest<BacktestResult>;

public sealed class BacktestStrategyValidator : AbstractValidator<BacktestStrategyCommand>
{
    public BacktestStrategyValidator()
    {
        RuleFor(x => x.StrategyId).NotEmpty();
        RuleFor(x => x.Symbol).NotEmpty();
        RuleFor(x => x.InitialCapital).GreaterThan(0);
        RuleFor(x => x.From).LessThan(x => x.To);
    }
}

public sealed class BacktestStrategyHandler(IUnitOfWork uow, IAiStrategyEngine aiEngine)
    : IRequestHandler<BacktestStrategyCommand, BacktestResult>
{
    public async Task<BacktestResult> Handle(BacktestStrategyCommand request, CancellationToken ct)
    {
        var strategy = await uow.Strategies.GetByIdAsync(request.StrategyId, ct)
            ?? throw new NotFoundException(nameof(TradingStrategy), request.StrategyId);

        var result = await aiEngine.RunBacktestAsync(new BacktestRequest(
            strategy.Id,
            request.Symbol,
            strategy.ParametersJson,
            request.From,
            request.To,
            request.InitialCapital), ct);

        return result;
    }
}
