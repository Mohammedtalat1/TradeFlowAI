using AutoMapper;
using MediatR;
using TradeFlowAI.Application.Common.Exceptions;
using TradeFlowAI.Application.Common.Interfaces;
using TradeFlowAI.Application.DTOs;
using TradeFlowAI.Domain.Aggregates.Portfolio;
using TradeFlowAI.Domain.Interfaces;

namespace TradeFlowAI.Application.Features.Portfolio.Commands;

public sealed record RebalancePortfolioCommand(Guid AccountId) : IRequest<PortfolioDto>;

public sealed class RebalancePortfolioHandler(
    IUnitOfWork uow,
    IMarketDataService marketData,
    IMapper mapper)
    : IRequestHandler<RebalancePortfolioCommand, PortfolioDto>
{
    public async Task<PortfolioDto> Handle(RebalancePortfolioCommand request, CancellationToken ct)
    {
        var portfolio = await uow.Portfolios.GetByAccountIdAsync(request.AccountId, ct)
            ?? throw new NotFoundException(nameof(Domain.Aggregates.Portfolio.Portfolio), request.AccountId);

        var priceDict = new Dictionary<string, decimal>();
        foreach (var position in portfolio.Positions)
        {
            var price = await marketData.GetCurrentPriceAsync(position.Symbol, ct);
            priceDict[position.Symbol] = price;
        }

        portfolio.UpdateMarketValues(priceDict);
        uow.Portfolios.Update(portfolio);
        await uow.SaveChangesAsync(ct);

        return mapper.Map<PortfolioDto>(portfolio);
    }
}
