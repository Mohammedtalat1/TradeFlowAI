using AutoMapper;
using MediatR;
using TradeFlowAI.Application.Common.Exceptions;
using TradeFlowAI.Application.DTOs;
using TradeFlowAI.Domain.Interfaces;

namespace TradeFlowAI.Application.Features.Funds.Queries;

public sealed record GetWalletBalanceQuery(Guid AccountId) : IRequest<WalletDto>;

public sealed class GetWalletBalanceHandler(IUnitOfWork uow, IMapper mapper)
    : IRequestHandler<GetWalletBalanceQuery, WalletDto>
{
    public async Task<WalletDto> Handle(GetWalletBalanceQuery request, CancellationToken ct)
    {
        var wallet = await uow.Wallets.GetByAccountIdAsync(request.AccountId, ct)
            ?? throw new NotFoundException("Wallet", request.AccountId);
        return mapper.Map<WalletDto>(wallet);
    }
}

public sealed record GetTransactionHistoryQuery(Guid AccountId, int Page = 1, int PageSize = 20)
    : IRequest<PagedResult<TransactionDto>>;

public sealed class GetTransactionHistoryHandler(IUnitOfWork uow, IMapper mapper)
    : IRequestHandler<GetTransactionHistoryQuery, PagedResult<TransactionDto>>
{
    public async Task<PagedResult<TransactionDto>> Handle(GetTransactionHistoryQuery request, CancellationToken ct)
    {
        var wallet = await uow.Wallets.GetByAccountIdAsync(request.AccountId, ct)
            ?? throw new NotFoundException("Wallet", request.AccountId);

        var txs = await uow.Wallets.GetTransactionsAsync(wallet.Id, request.Page, request.PageSize, ct);
        var dtos = mapper.Map<IReadOnlyList<TransactionDto>>(txs);

        return new PagedResult<TransactionDto>(dtos, dtos.Count, request.Page, request.PageSize);
    }
}
