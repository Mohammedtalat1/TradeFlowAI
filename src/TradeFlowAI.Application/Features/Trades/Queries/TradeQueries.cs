using AutoMapper;
using MediatR;
using TradeFlowAI.Application.Common.Exceptions;
using TradeFlowAI.Application.DTOs;
using TradeFlowAI.Domain.Interfaces;

namespace TradeFlowAI.Application.Features.Trades.Queries;

public sealed record GetTradeQuery(Guid TradeId) : IRequest<TradeDto>;

public sealed class GetTradeHandler(IUnitOfWork uow, IMapper mapper)
    : IRequestHandler<GetTradeQuery, TradeDto>
{
    public async Task<TradeDto> Handle(GetTradeQuery request, CancellationToken ct)
    {
        var trade = await uow.Trades.GetByIdAsync(request.TradeId, ct)
            ?? throw new NotFoundException(nameof(Domain.Aggregates.Trade.Trade), request.TradeId);
        return mapper.Map<TradeDto>(trade);
    }
}

public sealed record GetOpenTradesQuery(Guid AccountId) : IRequest<IReadOnlyList<TradeDto>>;

public sealed class GetOpenTradesHandler(IUnitOfWork uow, IMapper mapper)
    : IRequestHandler<GetOpenTradesQuery, IReadOnlyList<TradeDto>>
{
    public async Task<IReadOnlyList<TradeDto>> Handle(GetOpenTradesQuery request, CancellationToken ct)
    {
        var trades = await uow.Trades.GetOpenTradesByAccountAsync(request.AccountId, ct);
        return mapper.Map<IReadOnlyList<TradeDto>>(trades);
    }
}

public sealed record GetTradeHistoryQuery(Guid AccountId, int Page = 1, int PageSize = 20)
    : IRequest<PagedResult<TradeDto>>;

public sealed class GetTradeHistoryHandler(IUnitOfWork uow, IMapper mapper)
    : IRequestHandler<GetTradeHistoryQuery, PagedResult<TradeDto>>
{
    public async Task<PagedResult<TradeDto>> Handle(GetTradeHistoryQuery request, CancellationToken ct)
    {
        var trades = await uow.Trades.GetTradeHistoryAsync(request.AccountId, request.Page, request.PageSize, ct);
        var dtos = mapper.Map<IReadOnlyList<TradeDto>>(trades);
        return new PagedResult<TradeDto>(dtos, dtos.Count, request.Page, request.PageSize);
    }
}
