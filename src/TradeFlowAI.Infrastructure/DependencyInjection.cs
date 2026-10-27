using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TradeFlowAI.Application.Common.Interfaces;
using TradeFlowAI.Domain.Interfaces;
using TradeFlowAI.Infrastructure.Caching;
using TradeFlowAI.Infrastructure.ExternalServices.AI;
using TradeFlowAI.Infrastructure.ExternalServices.MarketData;
using TradeFlowAI.Infrastructure.ExternalServices.Notifications;
using TradeFlowAI.Infrastructure.ExternalServices.Payment;
using TradeFlowAI.Infrastructure.Identity;
using TradeFlowAI.Infrastructure.Persistence;
using TradeFlowAI.Infrastructure.Persistence.Repositories;
using TradeFlowAI.Infrastructure.TradingStrategies;

namespace TradeFlowAI.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // ── Persistence & Database ─────────────────────────────────────────
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        if (!string.IsNullOrEmpty(connectionString))
        {
            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(connectionString, b => b.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName)));
        }
        else
        {
            // Default to high-performance In-Memory Database for immediate plug-and-play testing
            services.AddDbContext<AppDbContext>(options =>
                options.UseInMemoryDatabase("TradeFlowAIDb"));
        }

        // ── Identity ───────────────────────────────────────────────────────
        services.AddIdentityCore<ApplicationUser>(options =>
        {
            options.Password.RequireDigit = false;
            options.Password.RequireLowercase = false;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequireUppercase = false;
            options.Password.RequiredLength = 6;
        })
        .AddRoles<IdentityRole>()
        .AddEntityFrameworkStores<AppDbContext>();

        // ── Repositories & Unit of Work ────────────────────────────────────
        services.AddScoped<IAccountRepository, AccountRepository>();
        services.AddScoped<IWalletRepository, WalletRepository>();
        services.AddScoped<IPortfolioRepository, PortfolioRepository>();
        services.AddScoped<ITradeRepository, TradeRepository>();
        services.AddScoped<IStrategyRepository, StrategyRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // ── Strategy Pattern & Factory ─────────────────────────────────────
        services.AddSingleton<IStrategyAlgorithm, MomentumStrategy>();
        services.AddSingleton<IStrategyAlgorithm, MeanReversionStrategy>();
        services.AddSingleton<IStrategyAlgorithm, AiDrivenStrategy>();
        services.AddSingleton<IStrategyFactory, StrategyFactory>();

        // ── AI Strategy Engine with Decorators (Decorator Pattern) ─────────
        services.AddScoped<AiStrategyEngine>();
        services.AddScoped<IAiStrategyEngine>(sp =>
        {
            var coreEngine = sp.GetRequiredService<AiStrategyEngine>();
            var loggingLogger = sp.GetRequiredService<ILogger<AiStrategyEngineLoggingDecorator>>();
            var retryLogger = sp.GetRequiredService<ILogger<AiStrategyEngineRetryDecorator>>();

            // Decorator pipeline: Core -> LoggingDecorator -> RetryDecorator
            var loggingDecorator = new AiStrategyEngineLoggingDecorator(coreEngine, loggingLogger);
            var retryDecorator = new AiStrategyEngineRetryDecorator(loggingDecorator, retryLogger, maxRetries: 3);

            return retryDecorator;
        });

        // ── External Service Adapters ──────────────────────────────────────
        services.AddSingleton<IMarketDataService, MarketDataService>();
        services.AddScoped<IPaymentGateway, PaymentGatewayService>();
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<IWebhookDispatcher, WebhookDispatcher>();
        services.AddSingleton<ICacheService, MemoryCacheService>();

        // ── Identity & Security ────────────────────────────────────────────
        services.AddSingleton<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IIdentityService, IdentityService>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();

        return services;
    }
}
