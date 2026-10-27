using TradeFlowAI.Domain.Aggregates.Account;
using TradeFlowAI.Domain.Aggregates.Fund;
using TradeFlowAI.Domain.Aggregates.Portfolio;
using TradeFlowAI.Domain.Aggregates.Strategy;

namespace TradeFlowAI.Domain.Interfaces;

// ── Repository interfaces (ISP — one per aggregate) ──────────────────────

public interface IReadRepository<T, TId>
{
    Task<T?> GetByIdAsync(TId id, CancellationToken ct = default);
    Task<IReadOnlyList<T>> GetAllAsync(CancellationToken ct = default);
}

public interface IWriteRepository<T, TId>
{
    Task AddAsync(T entity, CancellationToken ct = default);
    void Update(T entity);
    void Delete(T entity);
}

public interface IRepository<T, TId> : IReadRepository<T, TId>, IWriteRepository<T, TId>;

public interface IAccountRepository : IRepository<Account, Guid>
{
    Task<Account?> GetByUserIdAsync(string userId, CancellationToken ct = default);
    Task<IReadOnlyList<Account>> GetActiveAccountsAsync(CancellationToken ct = default);
}

public interface IWalletRepository : IRepository<Wallet, Guid>
{
    Task<Wallet?> GetByAccountIdAsync(Guid accountId, CancellationToken ct = default);
    Task<IReadOnlyList<Wallet.Transaction>> GetTransactionsAsync(Guid walletId, int page, int pageSize, CancellationToken ct = default);
}

public interface IPortfolioRepository : IRepository<Portfolio, Guid>
{
    Task<Portfolio?> GetByAccountIdAsync(Guid accountId, CancellationToken ct = default);
}

public interface ITradeRepository : IRepository<Domain.Aggregates.Trade.Trade, Guid>
{
    Task<IReadOnlyList<Domain.Aggregates.Trade.Trade>> GetOpenTradesByAccountAsync(Guid accountId, CancellationToken ct = default);
    Task<IReadOnlyList<Domain.Aggregates.Trade.Trade>> GetTradeHistoryAsync(Guid accountId, int page, int pageSize, CancellationToken ct = default);
    Task<int> GetOpenTradeCountAsync(Guid accountId, CancellationToken ct = default);
}

public interface IStrategyRepository : IRepository<TradingStrategy, Guid>
{
    Task<IReadOnlyList<TradingStrategy>> GetByUserAsync(string userId, CancellationToken ct = default);
    Task<TradingStrategy?> GetActiveByAccountAsync(Guid accountId, CancellationToken ct = default);
}

// Unit of Work — atomic multi-repository transactions
public interface IUnitOfWork
{
    IAccountRepository Accounts { get; }
    IWalletRepository Wallets { get; }
    IPortfolioRepository Portfolios { get; }
    ITradeRepository Trades { get; }
    IStrategyRepository Strategies { get; }

    Task<int> SaveChangesAsync(CancellationToken ct = default);
    Task BeginTransactionAsync(CancellationToken ct = default);
    Task CommitTransactionAsync(CancellationToken ct = default);
    Task RollbackTransactionAsync(CancellationToken ct = default);
}
