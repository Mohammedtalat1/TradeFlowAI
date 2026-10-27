using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TradeFlowAI.Domain.Aggregates.Account;
using TradeFlowAI.Domain.Aggregates.Fund;
using TradeFlowAI.Domain.Aggregates.Portfolio;
using TradeFlowAI.Domain.Aggregates.Strategy;
using TradeFlowAI.Domain.Aggregates.Trade;
using TradeFlowAI.Domain.Enums;
using TradeFlowAI.Infrastructure.Identity;

namespace TradeFlowAI.Infrastructure.Persistence;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(AppDbContext db, UserManager<ApplicationUser> userManager)
    {
        if (await db.Accounts.AnyAsync())
            return;

        // 1. Create Demo User
        var user = new ApplicationUser
        {
            UserName = "demo@tradeflow.ai",
            Email = "demo@tradeflow.ai",
            FullName = "Demo Trader",
            EmailConfirmed = true
        };

        var userResult = await userManager.CreateAsync(user, "Demo1234!");
        var userId = userResult.Succeeded ? user.Id : "demo-user-id";

        // 2. Create Strategy
        var strategyId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var strategy = TradingStrategy.Create(
            strategyId,
            "Autonomous Momentum Alpha",
            "Multi-timeframe SMA and volume expansion predictive momentum algorithm",
            StrategyType.Momentum,
            ["BTC/USD", "ETH/USD", "SOL/USD"],
            userId,
            "{\"shortPeriod\":5,\"longPeriod\":20,\"confidenceThreshold\":0.70}");
        strategy.Activate();
        await db.Strategies.AddAsync(strategy);

        // 3. Create Account
        var accountId = Guid.Parse("22222222-2222-2222-2222-222222222222");
        var account = Account.Create(accountId, userId, "Primary Autonomous Trading Account");
        account.Activate();
        account.EnableAutoTrading(strategy.Id);
        await db.Accounts.AddAsync(account);

        // 4. Create Wallet and Deposit $100,000
        var walletId = Guid.Parse("33333333-3333-3333-3333-333333333333");
        var wallet = Wallet.Create(walletId, accountId, "USD");
        wallet.Deposit(100000m, "Initial Capital Inflow");
        await db.Wallets.AddAsync(wallet);

        // 5. Create Portfolio & Positions
        var portfolioId = Guid.Parse("44444444-4444-4444-4444-444444444444");
        var portfolio = Portfolio.Create(portfolioId, accountId);
        portfolio.ApplyTradeFill("BTC/USD", 0.5m, 65000m, TradeDirection.Buy);
        portfolio.ApplyTradeFill("ETH/USD", 4.0m, 3200m, TradeDirection.Buy);
        portfolio.UpdateMarketValues(new Dictionary<string, decimal>
        {
            ["BTC/USD"] = 68450m,
            ["ETH/USD"] = 3520m
        });
        await db.Portfolios.AddAsync(portfolio);

        // 6. Create Completed Trades
        var trade1 = Trade.Create(
            Guid.NewGuid(), accountId, "BTC/USD", TradeDirection.Buy, OrderType.Market, 0.5m,
            strategyId: strategyId, isAiInitiated: true);
        trade1.Submit();
        trade1.Fill(65000m, 0.5m);
        await db.Trades.AddAsync(trade1);

        var trade2 = Trade.Create(
            Guid.NewGuid(), accountId, "ETH/USD", TradeDirection.Buy, OrderType.Market, 4.0m,
            strategyId: strategyId, isAiInitiated: true);
        trade2.Submit();
        trade2.Fill(3200m, 4.0m);
        await db.Trades.AddAsync(trade2);

        await db.SaveChangesAsync();
    }
}
