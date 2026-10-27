using AutoMapper;
using FluentValidation;
using MediatR;
using TradeFlowAI.Application.Common.Exceptions;
using TradeFlowAI.Application.Common.Interfaces;
using TradeFlowAI.Application.DTOs;
using TradeFlowAI.Domain.Interfaces;

namespace TradeFlowAI.Application.Features.Funds.Commands;

// ── Deposit Funds ──────────────────────────────────────────────────────────

public sealed record DepositFundsCommand(
    Guid AccountId,
    decimal Amount,
    string ExternalReference
) : IRequest<WalletDto>;

public sealed class DepositFundsValidator : AbstractValidator<DepositFundsCommand>
{
    public DepositFundsValidator()
    {
        RuleFor(x => x.AccountId).NotEmpty();
        RuleFor(x => x.Amount).GreaterThan(0).LessThanOrEqualTo(1_000_000);
        RuleFor(x => x.ExternalReference).NotEmpty().MaximumLength(200);
    }
}

public sealed class DepositFundsHandler(
    IUnitOfWork uow,
    IMapper mapper,
    IPaymentGateway paymentGateway,
    IWebhookDispatcher webhookDispatcher)
    : IRequestHandler<DepositFundsCommand, WalletDto>
{
    public async Task<WalletDto> Handle(DepositFundsCommand request, CancellationToken ct)
    {
        var wallet = await uow.Wallets.GetByAccountIdAsync(request.AccountId, ct)
            ?? throw new NotFoundException("Wallet", request.AccountId);

        // Process via payment gateway (adapter)
        var payment = await paymentGateway.ProcessDepositAsync(
            request.AccountId, request.Amount, wallet.Currency, request.ExternalReference, ct);

        if (!payment.Success)
            throw new InvalidOperationException($"Payment failed: {payment.ErrorMessage}");

        wallet.Deposit(request.Amount, payment.TransactionId);
        uow.Wallets.Update(wallet);
        await uow.SaveChangesAsync(ct);

        // Dispatch webhook to all subscribed integrations
        await webhookDispatcher.DispatchAsync("funds.deposited", new
        {
            accountId = request.AccountId,
            amount = request.Amount,
            transactionId = payment.TransactionId
        }, request.AccountId, ct);

        return mapper.Map<WalletDto>(wallet);
    }
}

// ── Withdraw Funds ─────────────────────────────────────────────────────────

public sealed record WithdrawFundsCommand(
    Guid AccountId,
    decimal Amount,
    string BankDetails
) : IRequest<WalletDto>;

public sealed class WithdrawFundsValidator : AbstractValidator<WithdrawFundsCommand>
{
    public WithdrawFundsValidator()
    {
        RuleFor(x => x.AccountId).NotEmpty();
        RuleFor(x => x.Amount).GreaterThan(0);
        RuleFor(x => x.BankDetails).NotEmpty().MaximumLength(500);
    }
}

public sealed class WithdrawFundsHandler(
    IUnitOfWork uow,
    IMapper mapper,
    IPaymentGateway paymentGateway,
    IWebhookDispatcher webhookDispatcher)
    : IRequestHandler<WithdrawFundsCommand, WalletDto>
{
    public async Task<WalletDto> Handle(WithdrawFundsCommand request, CancellationToken ct)
    {
        var wallet = await uow.Wallets.GetByAccountIdAsync(request.AccountId, ct)
            ?? throw new NotFoundException("Wallet", request.AccountId);

        wallet.Withdraw(request.Amount, "Withdrawal request");

        var payment = await paymentGateway.ProcessWithdrawalAsync(
            request.AccountId, request.Amount, wallet.Currency, request.BankDetails, ct);

        if (!payment.Success)
        {
            // Rollback the domain change by re-depositing
            wallet.Deposit(request.Amount, "Withdrawal reversal");
            throw new InvalidOperationException($"Withdrawal failed: {payment.ErrorMessage}");
        }

        uow.Wallets.Update(wallet);
        await uow.SaveChangesAsync(ct);

        await webhookDispatcher.DispatchAsync("funds.withdrawn", new
        {
            accountId = request.AccountId,
            amount = request.Amount,
            transactionId = payment.TransactionId
        }, request.AccountId, ct);

        return mapper.Map<WalletDto>(wallet);
    }
}
