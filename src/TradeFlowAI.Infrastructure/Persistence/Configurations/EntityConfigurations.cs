using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TradeFlowAI.Domain.Aggregates.Account;
using TradeFlowAI.Domain.Aggregates.Fund;
using TradeFlowAI.Domain.Aggregates.Portfolio;
using TradeFlowAI.Domain.Aggregates.Strategy;
using TradeFlowAI.Domain.Aggregates.Trade;

namespace TradeFlowAI.Infrastructure.Persistence.Configurations;

public sealed class AccountConfiguration : IEntityTypeConfiguration<Account>
{
    public void Configure(EntityTypeBuilder<Account> builder)
    {
        builder.HasKey(a => a.Id);
        builder.Property(a => a.UserId).IsRequired().HasMaxLength(128);
        builder.Property(a => a.Name).IsRequired().HasMaxLength(100);

        builder.OwnsOne(a => a.RiskProfile, rp =>
        {
            rp.Property(p => p.MaxDrawdownPercent).HasPrecision(5, 2).HasColumnName("Risk_MaxDrawdownPercent");
            rp.Property(p => p.MaxPositionSizePercent).HasPrecision(5, 2).HasColumnName("Risk_MaxPositionSizePercent");
            rp.Property(p => p.DailyLossLimitPercent).HasPrecision(5, 2).HasColumnName("Risk_DailyLossLimitPercent");
            rp.Property(p => p.MaxOpenTrades).HasColumnName("Risk_MaxOpenTrades");
        });
    }
}

public sealed class WalletConfiguration : IEntityTypeConfiguration<Wallet>
{
    public void Configure(EntityTypeBuilder<Wallet> builder)
    {
        builder.HasKey(w => w.Id);
        builder.Property(w => w.Currency).IsRequired().HasMaxLength(10);

        builder.OwnsOne(w => w.Balance, m =>
        {
            m.Property(p => p.Amount).HasPrecision(18, 4).HasColumnName("Balance_Amount");
            m.Property(p => p.Currency).HasMaxLength(10).HasColumnName("Balance_Currency");
        });

        builder.OwnsOne(w => w.ReservedBalance, m =>
        {
            m.Property(p => p.Amount).HasPrecision(18, 4).HasColumnName("ReservedBalance_Amount");
            m.Property(p => p.Currency).HasMaxLength(10).HasColumnName("ReservedBalance_Currency");
        });

        builder.HasMany(w => w.Transactions)
            .WithOne()
            .HasForeignKey(t => t.WalletId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(w => w.Transactions)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}

public sealed class TransactionConfiguration : IEntityTypeConfiguration<Transaction>
{
    public void Configure(EntityTypeBuilder<Transaction> builder)
    {
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Reference).HasMaxLength(256);

        builder.OwnsOne(t => t.Amount, m =>
        {
            m.Property(p => p.Amount).HasPrecision(18, 4).HasColumnName("Amount");
            m.Property(p => p.Currency).HasMaxLength(10).HasColumnName("Currency");
        });
    }
}

public sealed class PortfolioConfiguration : IEntityTypeConfiguration<Portfolio>
{
    public void Configure(EntityTypeBuilder<Portfolio> builder)
    {
        builder.HasKey(p => p.Id);
        builder.Property(p => p.TotalValue).HasPrecision(18, 4);
        builder.Property(p => p.TotalCost).HasPrecision(18, 4);
        builder.Property(p => p.RealizedPnl).HasPrecision(18, 4);

        builder.HasMany(p => p.Positions)
            .WithOne()
            .HasForeignKey(pos => pos.PortfolioId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(p => p.Positions)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}

public sealed class PositionConfiguration : IEntityTypeConfiguration<Position>
{
    public void Configure(EntityTypeBuilder<Position> builder)
    {
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Symbol).IsRequired().HasMaxLength(20);
        builder.Property(p => p.Quantity).HasPrecision(18, 8);
        builder.Property(p => p.AverageCost).HasPrecision(18, 4);
        builder.Property(p => p.CurrentPrice).HasPrecision(18, 4);
    }
}

public sealed class TradeConfiguration : IEntityTypeConfiguration<Trade>
{
    public void Configure(EntityTypeBuilder<Trade> builder)
    {
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Symbol).IsRequired().HasMaxLength(20);
        builder.Property(t => t.Quantity).HasPrecision(18, 8);
        builder.Property(t => t.FilledQuantity).HasPrecision(18, 8);
        builder.Property(t => t.LimitPrice).HasPrecision(18, 4);
        builder.Property(t => t.ExecutionPrice).HasPrecision(18, 4);
        builder.Property(t => t.CancellationReason).HasMaxLength(500);
    }
}

public sealed class TradingStrategyConfiguration : IEntityTypeConfiguration<TradingStrategy>
{
    public void Configure(EntityTypeBuilder<TradingStrategy> builder)
    {
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Name).IsRequired().HasMaxLength(100);
        builder.Property(s => s.Description).HasMaxLength(500);
        builder.Property(s => s.CreatedByUserId).IsRequired().HasMaxLength(128);

        builder.Property(s => s.SupportedSymbols)
            .HasConversion(
                v => string.Join(',', v),
                v => v.Split(',', StringSplitOptions.RemoveEmptyEntries));
    }
}
