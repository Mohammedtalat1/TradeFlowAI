using AutoMapper;
using MediatR;
using TradeFlowAI.Application.Common.Exceptions;
using TradeFlowAI.Application.DTOs;
using TradeFlowAI.Domain.Interfaces;

namespace TradeFlowAI.Application.Features.Accounts.Queries;

// ── Get Account ────────────────────────────────────────────────────────────

public sealed record GetAccountQuery(Guid AccountId) : IRequest<AccountDto>;

public sealed class GetAccountHandler(IUnitOfWork uow, IMapper mapper)
    : IRequestHandler<GetAccountQuery, AccountDto>
{
    public async Task<AccountDto> Handle(GetAccountQuery request, CancellationToken ct)
    {
        var account = await uow.Accounts.GetByIdAsync(request.AccountId, ct)
            ?? throw new NotFoundException(nameof(Domain.Aggregates.Account.Account), request.AccountId);

        return mapper.Map<AccountDto>(account);
    }
}

// ── Get All Accounts (Admin) ───────────────────────────────────────────────

public sealed record GetAccountsQuery : IRequest<IReadOnlyList<AccountDto>>;

public sealed class GetAccountsHandler(IUnitOfWork uow, IMapper mapper)
    : IRequestHandler<GetAccountsQuery, IReadOnlyList<AccountDto>>
{
    public async Task<IReadOnlyList<AccountDto>> Handle(GetAccountsQuery request, CancellationToken ct)
    {
        var accounts = await uow.Accounts.GetAllAsync(ct);
        return mapper.Map<IReadOnlyList<AccountDto>>(accounts);
    }
}
