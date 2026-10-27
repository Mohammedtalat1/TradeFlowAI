using AutoMapper;
using TradeFlowAI.Application.DTOs;
using TradeFlowAI.Domain.Aggregates.Account;
using TradeFlowAI.Domain.Aggregates.Fund;
using TradeFlowAI.Domain.Aggregates.Portfolio;
using TradeFlowAI.Domain.Aggregates.Strategy;

namespace TradeFlowAI.Application.Common.Mappings;

public sealed class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Account, AccountDto>();

        CreateMap<Wallet, WalletDto>()
            .ForMember(d => d.Balance, o => o.MapFrom(s => s.Balance.Amount))
            .ForMember(d => d.ReservedBalance, o => o.MapFrom(s => s.ReservedBalance.Amount))
            .ForMember(d => d.AvailableBalance, o => o.MapFrom(s => s.AvailableBalance.Amount))
            .ForMember(d => d.Currency, o => o.MapFrom(s => s.Currency));

        CreateMap<Transaction, TransactionDto>()
            .ForMember(d => d.Amount, o => o.MapFrom(s => s.Amount.Amount))
            .ForMember(d => d.Currency, o => o.MapFrom(s => s.Amount.Currency));

        CreateMap<Portfolio, PortfolioDto>();
        CreateMap<Position, PositionDto>();

        CreateMap<Domain.Aggregates.Trade.Trade, TradeDto>();

        CreateMap<TradingStrategy, TradingStrategyDto>();
    }
}
