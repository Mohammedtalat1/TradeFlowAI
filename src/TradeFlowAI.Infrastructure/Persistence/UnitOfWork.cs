using Microsoft.EntityFrameworkCore.Storage;
using TradeFlowAI.Domain.Interfaces;
using TradeFlowAI.Infrastructure.Persistence.Repositories;

namespace TradeFlowAI.Infrastructure.Persistence;

public sealed class UnitOfWork(
    AppDbContext db,
    IAccountRepository accounts,
    IWalletRepository wallets,
    IPortfolioRepository portfolios,
    ITradeRepository trades,
    IStrategyRepository strategies)
    : IUnitOfWork
{
    private IDbContextTransaction? _currentTransaction;

    public IAccountRepository Accounts => accounts;
    public IWalletRepository Wallets => wallets;
    public IPortfolioRepository Portfolios => portfolios;
    public ITradeRepository Trades => trades;
    public IStrategyRepository Strategies => strategies;

    public async Task<int> SaveChangesAsync(CancellationToken ct = default)
        => await db.SaveChangesAsync(ct);

    public async Task BeginTransactionAsync(CancellationToken ct = default)
    {
        if (_currentTransaction is not null) return;
        _currentTransaction = await db.Database.BeginTransactionAsync(ct);
    }

    public async Task CommitTransactionAsync(CancellationToken ct = default)
    {
        try
        {
            await db.SaveChangesAsync(ct);
            if (_currentTransaction is not null)
            {
                await _currentTransaction.CommitAsync(ct);
            }
        }
        catch
        {
            await RollbackTransactionAsync(ct);
            throw;
        }
        finally
        {
            if (_currentTransaction is not null)
            {
                await _currentTransaction.DisposeAsync();
                _currentTransaction = null;
            }
        }
    }

    public async Task RollbackTransactionAsync(CancellationToken ct = default)
    {
        try
        {
            if (_currentTransaction is not null)
            {
                await _currentTransaction.RollbackAsync(ct);
            }
        }
        finally
        {
            if (_currentTransaction is not null)
            {
                await _currentTransaction.DisposeAsync();
                _currentTransaction = null;
            }
        }
    }
}
