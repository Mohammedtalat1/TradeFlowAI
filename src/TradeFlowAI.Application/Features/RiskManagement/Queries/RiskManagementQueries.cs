using MediatR;
using TradeFlowAI.Application.Common.Exceptions;
using TradeFlowAI.Application.DTOs;
using TradeFlowAI.Domain.Aggregates.Account;
using TradeFlowAI.Domain.Interfaces;

namespace TradeFlowAI.Application.Features.RiskManagement.Queries;

public sealed record GetRiskMetricsQuery(Guid AccountId) : IRequest<RiskMetricsDto>;

public sealed class GetRiskMetricsHandler(IUnitOfWork uow)
    : IRequestHandler<GetRiskMetricsQuery, RiskMetricsDto>
{
    public async Task<RiskMetricsDto> Handle(GetRiskMetricsQuery request, CancellationToken ct)
    {
        var account = await uow.Accounts.GetByIdAsync(request.AccountId, ct)
            ?? throw new NotFoundException(nameof(Account), request.AccountId);

        var portfolio = await uow.Portfolios.GetByAccountIdAsync(request.AccountId, ct);
        var openTradesCount = await uow.Trades.GetOpenTradeCountAsync(request.AccountId, ct);

        decimal currentDrawdown = 0m;
        decimal dailyLossPercent = 0m;
        decimal totalExposure = portfolio?.TotalCost ?? 0m;

        if (portfolio != null && portfolio.TotalCost > 0)
        {
            if (portfolio.UnrealizedPnl < 0)
                currentDrawdown = Math.Abs(portfolio.UnrealizedPnl) / portfolio.TotalCost * 100m;
        }

        bool isCircuitBreaker = currentDrawdown >= account.RiskProfile.MaxDrawdownPercent;

        return new RiskMetricsDto(
            account.Id,
            Math.Round(currentDrawdown, 2),
            Math.Round(dailyLossPercent, 2),
            openTradesCount,
            totalExposure,
            isCircuitBreaker);
    }
}

public sealed record CircuitBreakerStatusDto(
    Guid AccountId,
    bool IsTripped,
    string Reason,
    DateTime? TrippedAt);

public sealed record GetCircuitBreakerStatusQuery(Guid AccountId) : IRequest<CircuitBreakerStatusDto>;

public sealed class GetCircuitBreakerStatusHandler(IUnitOfWork uow)
    : IRequestHandler<GetCircuitBreakerStatusQuery, CircuitBreakerStatusDto>
{
    public async Task<CircuitBreakerStatusDto> Handle(GetCircuitBreakerStatusQuery request, CancellationToken ct)
    {
        var account = await uow.Accounts.GetByIdAsync(request.AccountId, ct)
            ?? throw new NotFoundException(nameof(Account), request.AccountId);

        var portfolio = await uow.Portfolios.GetByAccountIdAsync(request.AccountId, ct);
        decimal currentDrawdown = 0m;
        if (portfolio != null && portfolio.TotalCost > 0 && portfolio.UnrealizedPnl < 0)
        {
            currentDrawdown = Math.Abs(portfolio.UnrealizedPnl) / portfolio.TotalCost * 100m;
        }

        bool tripped = currentDrawdown >= account.RiskProfile.MaxDrawdownPercent;
        string reason = tripped ? $"Drawdown ({currentDrawdown:F1}%) exceeded limit ({account.RiskProfile.MaxDrawdownPercent}%)" : "Normal";

        return new CircuitBreakerStatusDto(
            account.Id,
            tripped,
            reason,
            tripped ? DateTime.UtcNow : null);
    }
}

public sealed record RiskLimitsDto(
    Guid AccountId,
    decimal MaxDrawdownPercent,
    decimal MaxPositionSizePercent,
    decimal DailyLossLimitPercent,
    int MaxOpenTrades);

public sealed record GetRiskLimitsQuery(Guid AccountId) : IRequest<RiskLimitsDto>;

public sealed class GetRiskLimitsHandler(IUnitOfWork uow)
    : IRequestHandler<GetRiskLimitsQuery, RiskLimitsDto>
{
    public async Task<RiskLimitsDto> Handle(GetRiskLimitsQuery request, CancellationToken ct)
    {
        var account = await uow.Accounts.GetByIdAsync(request.AccountId, ct)
            ?? throw new NotFoundException(nameof(Account), request.AccountId);

        return new RiskLimitsDto(
            account.Id,
            account.RiskProfile.MaxDrawdownPercent,
            account.RiskProfile.MaxPositionSizePercent,
            account.RiskProfile.DailyLossLimitPercent,
            account.RiskProfile.MaxOpenTrades);
    }
}
