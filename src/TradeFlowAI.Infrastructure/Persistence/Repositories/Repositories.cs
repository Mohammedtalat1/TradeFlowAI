using Microsoft.EntityFrameworkCore;
using TradeFlowAI.Domain.Aggregates.Account;
using TradeFlowAI.Domain.Aggregates.Fund;
using TradeFlowAI.Domain.Aggregates.Portfolio;
using TradeFlowAI.Domain.Aggregates.Strategy;
using TradeFlowAI.Domain.Aggregates.Trade;
using TradeFlowAI.Domain.Enums;
using TradeFlowAI.Domain.Interfaces;

namespace TradeFlowAI.Infrastructure.Persistence.Repositories;

public abstract class BaseRepository<T, TId>(AppDbContext db) : IRepository<T, TId> where T : class
{
    protected readonly AppDbContext Db = db;

    public virtual async Task<T?> GetByIdAsync(TId id, CancellationToken ct = default)
        => await Db.Set<T>().FindAsync([id], ct);

    public virtual async Task<IReadOnlyList<T>> GetAllAsync(CancellationToken ct = default)
        => await Db.Set<T>().ToListAsync(ct);

    public virtual async Task AddAsync(T entity, CancellationToken ct = default)
        => await Db.Set<T>().AddAsync(entity, ct);

    public virtual void Update(T entity)
        => Db.Set<T>().Update(entity);

    public virtual void Delete(T entity)
        => Db.Set<T>().Remove(entity);
}

public sealed class AccountRepository(AppDbContext db) : BaseRepository<Account, Guid>(db), IAccountRepository
{
    public async Task<Account?> GetByUserIdAsync(string userId, CancellationToken ct = default)
        => await Db.Accounts.FirstOrDefaultAsync(a => a.UserId == userId, ct);

    public async Task<IReadOnlyList<Account>> GetActiveAccountsAsync(CancellationToken ct = default)
        => await Db.Accounts.Where(a => a.Status == AccountStatus.Active).ToListAsync(ct);
}

public sealed class WalletRepository(AppDbContext db) : BaseRepository<Wallet, Guid>(db), IWalletRepository
{
    public override async Task<Wallet?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await Db.Wallets.Include(w => w.Transactions).FirstOrDefaultAsync(w => w.Id == id, ct);

    public async Task<Wallet?> GetByAccountIdAsync(Guid accountId, CancellationToken ct = default)
        => await Db.Wallets.Include(w => w.Transactions).FirstOrDefaultAsync(w => w.AccountId == accountId, ct);

    public async Task<IReadOnlyList<Transaction>> GetTransactionsAsync(
        Guid walletId, int page, int pageSize, CancellationToken ct = default)
    {
        var wallet = await Db.Wallets.Include(w => w.Transactions).FirstOrDefaultAsync(w => w.Id == walletId, ct);
        if (wallet is null) return [];

        return wallet.Transactions
            .OrderByDescending(t => t.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();
    }
}

public sealed class PortfolioRepository(AppDbContext db) : BaseRepository<Portfolio, Guid>(db), IPortfolioRepository
{
    public override async Task<Portfolio?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await Db.Portfolios.Include(p => p.Positions).FirstOrDefaultAsync(p => p.Id == id, ct);

    public async Task<Portfolio?> GetByAccountIdAsync(Guid accountId, CancellationToken ct = default)
        => await Db.Portfolios.Include(p => p.Positions).FirstOrDefaultAsync(p => p.AccountId == accountId, ct);
}

public sealed class TradeRepository(AppDbContext db) : BaseRepository<Trade, Guid>(db), ITradeRepository
{
    public async Task<IReadOnlyList<Trade>> GetOpenTradesByAccountAsync(Guid accountId, CancellationToken ct = default)
    {
        var openStatuses = new[] { TradeStatus.Pending, TradeStatus.Submitted, TradeStatus.PartiallyFilled };
        return await Db.Trades
            .Where(t => t.AccountId == accountId && openStatuses.Contains(t.Status))
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<Trade>> GetTradeHistoryAsync(
        Guid accountId, int page, int pageSize, CancellationToken ct = default)
    {
        return await Db.Trades
            .Where(t => t.AccountId == accountId)
            .OrderByDescending(t => t.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);
    }

    public async Task<int> GetOpenTradeCountAsync(Guid accountId, CancellationToken ct = default)
    {
        var openStatuses = new[] { TradeStatus.Pending, TradeStatus.Submitted, TradeStatus.PartiallyFilled };
        return await Db.Trades
            .CountAsync(t => t.AccountId == accountId && openStatuses.Contains(t.Status), ct);
    }
}

public sealed class StrategyRepository(AppDbContext db) : BaseRepository<TradingStrategy, Guid>(db), IStrategyRepository
{
    public async Task<IReadOnlyList<TradingStrategy>> GetByUserAsync(string userId, CancellationToken ct = default)
        => await Db.Strategies.Where(s => s.CreatedByUserId == userId).ToListAsync(ct);

    public async Task<TradingStrategy?> GetActiveByAccountAsync(Guid accountId, CancellationToken ct = default)
    {
        var account = await Db.Accounts.FirstOrDefaultAsync(a => a.Id == accountId, ct);
        if (account?.ActiveStrategyId is null) return null;

        return await Db.Strategies.FirstOrDefaultAsync(s => s.Id == account.ActiveStrategyId.Value, ct);
    }
}
