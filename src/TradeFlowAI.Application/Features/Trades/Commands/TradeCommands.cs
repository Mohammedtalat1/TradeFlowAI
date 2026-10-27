using AutoMapper;
using FluentValidation;
using MediatR;
using TradeFlowAI.Application.Common.Exceptions;
using TradeFlowAI.Application.Common.Interfaces;
using TradeFlowAI.Application.DTOs;
using TradeFlowAI.Domain.Aggregates.Trade;
using TradeFlowAI.Domain.Enums;
using TradeFlowAI.Domain.Interfaces;

namespace TradeFlowAI.Application.Features.Trades.Commands;

// ── Execute Trade (Manual) ─────────────────────────────────────────────────

public sealed record ExecuteTradeCommand(
    Guid AccountId,
    string Symbol,
    TradeDirection Direction,
    OrderType OrderType,
    decimal Quantity,
    decimal? LimitPrice = null
) : IRequest<TradeDto>;

public sealed class ExecuteTradeValidator : AbstractValidator<ExecuteTradeCommand>
{
    public ExecuteTradeValidator()
    {
        RuleFor(x => x.AccountId).NotEmpty();
        RuleFor(x => x.Symbol).NotEmpty().MaximumLength(20);
        RuleFor(x => x.Quantity).GreaterThan(0);
        RuleFor(x => x.LimitPrice).GreaterThan(0).When(x => x.OrderType == OrderType.Limit);
    }
}

public sealed class ExecuteTradeHandler(
    IUnitOfWork uow,
    IMapper mapper,
    IMarketDataService marketData,
    IWebhookDispatcher webhookDispatcher)
    : IRequestHandler<ExecuteTradeCommand, TradeDto>
{
    public async Task<TradeDto> Handle(ExecuteTradeCommand request, CancellationToken ct)
    {
        var account = await uow.Accounts.GetByIdAsync(request.AccountId, ct)
            ?? throw new NotFoundException(nameof(Domain.Aggregates.Account.Account), request.AccountId);

        var wallet = await uow.Wallets.GetByAccountIdAsync(request.AccountId, ct)
            ?? throw new NotFoundException("Wallet", request.AccountId);

        var portfolio = await uow.Portfolios.GetByAccountIdAsync(request.AccountId, ct)
            ?? throw new NotFoundException("Portfolio", request.AccountId);

        // Risk check: open trade count
        var openTradeCount = await uow.Trades.GetOpenTradeCountAsync(request.AccountId, ct);
        if (openTradeCount >= account.RiskProfile.MaxOpenTrades)
            throw new Domain.Exceptions.RiskLimitExceededException("MaxOpenTrades", openTradeCount, account.RiskProfile.MaxOpenTrades);

        // Get current market price for margin reservation
        var price = request.LimitPrice
            ?? await marketData.GetCurrentPriceAsync(request.Symbol, ct);

        var required = price * request.Quantity / account.RiskProfile.LeverageMultiplier;
        wallet.Reserve((decimal)required);

        var trade = Trade.Create(
            Guid.NewGuid(), request.AccountId,
            request.Symbol, request.Direction, request.OrderType,
            request.Quantity, request.LimitPrice);

        trade.Submit();

        // For market orders, simulate immediate fill
        if (request.OrderType == OrderType.Market)
        {
            var executionPrice = await marketData.GetCurrentPriceAsync(request.Symbol, ct);
            trade.Fill(executionPrice, request.Quantity);
            wallet.ReleaseReservation((decimal)required);
            portfolio.ApplyTradeFill(request.Symbol, request.Quantity, executionPrice, request.Direction);
        }

        await uow.Trades.AddAsync(trade, ct);
        uow.Wallets.Update(wallet);
        uow.Portfolios.Update(portfolio);
        await uow.SaveChangesAsync(ct);

        if (trade.Status == TradeStatus.Filled)
        {
            await webhookDispatcher.DispatchAsync("trade.executed", new
            {
                tradeId = trade.Id,
                accountId = trade.AccountId,
                symbol = trade.Symbol,
                direction = trade.Direction.ToString(),
                quantity = trade.Quantity,
                price = trade.ExecutionPrice
            }, request.AccountId, ct);
        }

        return mapper.Map<TradeDto>(trade);
    }
}

// ── AI-Triggered Trade ─────────────────────────────────────────────────────

public sealed record AiTriggerTradeCommand(
    Guid AccountId,
    Guid StrategyId,
    string Symbol,
    string Signal,         // "BUY" | "SELL"
    decimal Confidence,
    decimal SuggestedQuantity,
    decimal? SuggestedPrice
) : IRequest<TradeDto>;

public sealed class AiTriggerTradeHandler(
    IUnitOfWork uow,
    IMapper mapper,
    IMarketDataService marketData,
    IWebhookDispatcher webhookDispatcher)
    : IRequestHandler<AiTriggerTradeCommand, TradeDto>
{
    public async Task<TradeDto> Handle(AiTriggerTradeCommand request, CancellationToken ct)
    {
        // Only execute if confidence meets threshold
        if (request.Confidence < 0.65m)
            throw new InvalidOperationException($"AI confidence {request.Confidence} below threshold 0.65. Trade not executed.");

        var direction = request.Signal == "BUY" ? TradeDirection.Buy : TradeDirection.Sell;

        var delegatedCommand = new ExecuteTradeCommand(
            request.AccountId,
            request.Symbol,
            direction,
            OrderType.Market,
            request.SuggestedQuantity,
            request.SuggestedPrice);

        // Re-use the execute handler logic via mediator-like delegation
        var account = await uow.Accounts.GetByIdAsync(request.AccountId, ct)
            ?? throw new NotFoundException(nameof(Domain.Aggregates.Account.Account), request.AccountId);

        var wallet = await uow.Wallets.GetByAccountIdAsync(request.AccountId, ct)
            ?? throw new NotFoundException("Wallet", request.AccountId);

        var portfolio = await uow.Portfolios.GetByAccountIdAsync(request.AccountId, ct)
            ?? throw new NotFoundException("Portfolio", request.AccountId);

        var price = await marketData.GetCurrentPriceAsync(request.Symbol, ct);
        var required = price * request.SuggestedQuantity / account.RiskProfile.LeverageMultiplier;
        wallet.Reserve((decimal)required);

        var trade = Trade.Create(
            Guid.NewGuid(), request.AccountId,
            request.Symbol, direction, OrderType.Market,
            request.SuggestedQuantity, null,
            request.StrategyId, isAiInitiated: true);

        trade.Submit();
        var executionPrice = await marketData.GetCurrentPriceAsync(request.Symbol, ct);
        trade.Fill(executionPrice, request.SuggestedQuantity);
        wallet.ReleaseReservation((decimal)required);
        portfolio.ApplyTradeFill(request.Symbol, request.SuggestedQuantity, executionPrice, direction);

        await uow.Trades.AddAsync(trade, ct);
        uow.Wallets.Update(wallet);
        uow.Portfolios.Update(portfolio);
        await uow.SaveChangesAsync(ct);

        return mapper.Map<TradeDto>(trade);
    }
}

// ── Cancel Trade ───────────────────────────────────────────────────────────

public sealed record CancelTradeCommand(Guid TradeId, Guid AccountId, string Reason) : IRequest<TradeDto>;

public sealed class CancelTradeHandler(
    IUnitOfWork uow,
    IMapper mapper,
    IWebhookDispatcher webhookDispatcher)
    : IRequestHandler<CancelTradeCommand, TradeDto>
{
    public async Task<TradeDto> Handle(CancelTradeCommand request, CancellationToken ct)
    {
        var trade = await uow.Trades.GetByIdAsync(request.TradeId, ct)
            ?? throw new NotFoundException(nameof(Trade), request.TradeId);

        if (trade.AccountId != request.AccountId)
            throw new ForbiddenException("Trade does not belong to this account.");

        trade.Cancel(request.Reason);
        uow.Trades.Update(trade);
        await uow.SaveChangesAsync(ct);

        await webhookDispatcher.DispatchAsync("trade.cancelled", new
        {
            tradeId = trade.Id,
            reason = request.Reason
        }, request.AccountId, ct);

        return mapper.Map<TradeDto>(trade);
    }
}
