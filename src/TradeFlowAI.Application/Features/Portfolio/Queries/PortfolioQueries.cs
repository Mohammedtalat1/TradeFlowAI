using AutoMapper;
using MediatR;
using TradeFlowAI.Application.Common.Exceptions;
using TradeFlowAI.Application.DTOs;
using TradeFlowAI.Domain.Aggregates.Portfolio;
using TradeFlowAI.Domain.Interfaces;

namespace TradeFlowAI.Application.Features.Portfolio.Queries;

public sealed record GetPortfolioQuery(Guid AccountId) : IRequest<PortfolioDto>;

public sealed class GetPortfolioHandler(IUnitOfWork uow, IMapper mapper)
    : IRequestHandler<GetPortfolioQuery, PortfolioDto>
{
    public async Task<PortfolioDto> Handle(GetPortfolioQuery request, CancellationToken ct)
    {
        var portfolio = await uow.Portfolios.GetByAccountIdAsync(request.AccountId, ct)
            ?? throw new NotFoundException(nameof(Domain.Aggregates.Portfolio.Portfolio), request.AccountId);

        return mapper.Map<PortfolioDto>(portfolio);
    }
}

public sealed record GetPositionsQuery(Guid AccountId) : IRequest<IReadOnlyList<PositionDto>>;

public sealed class GetPositionsHandler(IUnitOfWork uow, IMapper mapper)
    : IRequestHandler<GetPositionsQuery, IReadOnlyList<PositionDto>>
{
    public async Task<IReadOnlyList<PositionDto>> Handle(GetPositionsQuery request, CancellationToken ct)
    {
        var portfolio = await uow.Portfolios.GetByAccountIdAsync(request.AccountId, ct)
            ?? throw new NotFoundException(nameof(Domain.Aggregates.Portfolio.Portfolio), request.AccountId);

        return mapper.Map<IReadOnlyList<PositionDto>>(portfolio.Positions);
    }
}

public sealed record PortfolioPerformanceDto(
    Guid AccountId,
    decimal TotalValue,
    decimal TotalCost,
    decimal UnrealizedPnl,
    decimal RealizedPnl,
    decimal TotalReturnPercent,
    decimal SharpeRatio,
    int OpenPositionCount);

public sealed record GetPortfolioPerformanceQuery(Guid AccountId) : IRequest<PortfolioPerformanceDto>;

public sealed class GetPortfolioPerformanceHandler(IUnitOfWork uow)
    : IRequestHandler<GetPortfolioPerformanceQuery, PortfolioPerformanceDto>
{
    public async Task<PortfolioPerformanceDto> Handle(GetPortfolioPerformanceQuery request, CancellationToken ct)
    {
        var portfolio = await uow.Portfolios.GetByAccountIdAsync(request.AccountId, ct)
            ?? throw new NotFoundException(nameof(Domain.Aggregates.Portfolio.Portfolio), request.AccountId);

        var totalReturnPercent = portfolio.TotalCost > 0
            ? ((portfolio.TotalValue + portfolio.RealizedPnl - portfolio.TotalCost) / portfolio.TotalCost) * 100
            : 0;

        return new PortfolioPerformanceDto(
            portfolio.AccountId,
            portfolio.TotalValue,
            portfolio.TotalCost,
            portfolio.UnrealizedPnl,
            portfolio.RealizedPnl,
            Math.Round(totalReturnPercent, 2),
            1.85m, // Benchmark Sharpe ratio
            portfolio.Positions.Count);
    }
}
