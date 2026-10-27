using MediatR;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TradeFlowAI.Domain.Aggregates.Account;
using TradeFlowAI.Domain.Aggregates.Fund;
using TradeFlowAI.Domain.Aggregates.Portfolio;
using TradeFlowAI.Domain.Aggregates.Strategy;
using TradeFlowAI.Domain.Aggregates.Trade;
using TradeFlowAI.Domain.Common;
using TradeFlowAI.Infrastructure.Identity;

namespace TradeFlowAI.Infrastructure.Persistence;

public class AppDbContext(
    DbContextOptions<AppDbContext> options,
    IPublisher? publisher = null)
    : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<Account> Accounts => Set<Account>();
    public DbSet<Wallet> Wallets => Set<Wallet>();
    public DbSet<Portfolio> Portfolios => Set<Portfolio>();
    public DbSet<Position> Positions => Set<Position>();
    public DbSet<Trade> Trades => Set<Trade>();
    public DbSet<TradingStrategy> Strategies => Set<TradingStrategy>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Dispatch domain events before committing (Observer / Domain Events pattern)
        if (publisher is not null)
        {
            var domainEntities = ChangeTracker
                .Entries()
                .Where(x => x.Entity is IHasDomainEvents)
                .Select(x => (IHasDomainEvents)x.Entity)
                .ToList();

            var domainEvents = domainEntities
                .SelectMany(x => x.DomainEvents)
                .ToList();

            foreach (var entity in domainEntities)
            {
                entity.ClearDomainEvents();
            }

            foreach (var domainEvent in domainEvents)
            {
                if (domainEvent is INotification notification)
                {
                    await publisher.Publish(notification, cancellationToken);
                }
            }
        }

        return await base.SaveChangesAsync(cancellationToken);
    }
}
