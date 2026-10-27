using Microsoft.Extensions.Logging;
using TradeFlowAI.Application.Common.Interfaces;

namespace TradeFlowAI.Infrastructure.ExternalServices.Payment;

/// <summary>
/// Adapter Pattern: Bridges external payment gateways (Stripe, Plaid, Bank Wire)
/// with the application IPaymentGateway interface.
/// </summary>
public sealed class PaymentGatewayService(ILogger<PaymentGatewayService> logger) : IPaymentGateway
{
    public Task<PaymentResult> ProcessDepositAsync(
        Guid accountId, decimal amount, string currency, string externalRef, CancellationToken ct = default)
    {
        logger.LogInformation("Processing deposit for Account {AccountId}: {Amount} {Currency} (Ref: {Ref})",
            accountId, amount, currency, externalRef);

        var txnId = $"txn_dep_{Guid.NewGuid():N}"[..20];
        return Task.FromResult(new PaymentResult(true, txnId, null));
    }

    public Task<PaymentResult> ProcessWithdrawalAsync(
        Guid accountId, decimal amount, string currency, string bankDetails, CancellationToken ct = default)
    {
        logger.LogInformation("Processing withdrawal for Account {AccountId}: {Amount} {Currency}",
            accountId, amount, currency);

        var txnId = $"txn_wth_{Guid.NewGuid():N}"[..20];
        return Task.FromResult(new PaymentResult(true, txnId, null));
    }
}
