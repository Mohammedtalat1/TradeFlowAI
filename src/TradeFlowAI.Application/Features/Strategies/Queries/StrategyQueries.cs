using AutoMapper;
using MediatR;
using TradeFlowAI.Application.Common.Exceptions;
using TradeFlowAI.Application.Common.Interfaces;
using TradeFlowAI.Application.DTOs;
using TradeFlowAI.Domain.Aggregates.Strategy;
using TradeFlowAI.Domain.Interfaces;

namespace TradeFlowAI.Application.Features.Strategies.Queries;

public sealed record GetStrategyQuery(Guid Id) : IRequest<TradingStrategyDto>;

public sealed class GetStrategyHandler(IUnitOfWork uow, IMapper mapper)
    : IRequestHandler<GetStrategyQuery, TradingStrategyDto>
{
    public async Task<TradingStrategyDto> Handle(GetStrategyQuery request, CancellationToken ct)
    {
        var strategy = await uow.Strategies.GetByIdAsync(request.Id, ct)
            ?? throw new NotFoundException(nameof(TradingStrategy), request.Id);

        return mapper.Map<TradingStrategyDto>(strategy);
    }
}

public sealed record GetStrategiesQuery(string? UserId = null) : IRequest<IReadOnlyList<TradingStrategyDto>>;

public sealed class GetStrategiesHandler(IUnitOfWork uow, IMapper mapper)
    : IRequestHandler<GetStrategiesQuery, IReadOnlyList<TradingStrategyDto>>
{
    public async Task<IReadOnlyList<TradingStrategyDto>> Handle(GetStrategiesQuery request, CancellationToken ct)
    {
        var strategies = string.IsNullOrEmpty(request.UserId)
            ? await uow.Strategies.GetAllAsync(ct)
            : await uow.Strategies.GetByUserAsync(request.UserId, ct);

        return mapper.Map<IReadOnlyList<TradingStrategyDto>>(strategies);
    }
}

public sealed record GetStrategySignalsQuery(Guid StrategyId, string Symbol) : IRequest<AiSignalResult>;

public sealed class GetStrategySignalsHandler(
    IUnitOfWork uow,
    IAiStrategyEngine aiEngine,
    IMarketDataService marketData)
    : IRequestHandler<GetStrategySignalsQuery, AiSignalResult>
{
    public async Task<AiSignalResult> Handle(GetStrategySignalsQuery request, CancellationToken ct)
    {
        var strategy = await uow.Strategies.GetByIdAsync(request.StrategyId, ct)
            ?? throw new NotFoundException(nameof(TradingStrategy), request.StrategyId);

        var currentPrice = await marketData.GetCurrentPriceAsync(request.Symbol, ct);
        var historicalBars = await marketData.GetHistoricalBarsAsync(request.Symbol, "1h", 50, ct);

        var signal = await aiEngine.GenerateSignalAsync(new AiSignalRequest(
            strategy.Id,
            request.Symbol,
            strategy.ParametersJson,
            currentPrice,
            historicalBars), ct);

        return signal;
    }
}
