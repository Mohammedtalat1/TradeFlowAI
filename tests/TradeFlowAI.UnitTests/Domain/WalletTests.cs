using FluentAssertions;
using TradeFlowAI.Domain.Aggregates.Fund;
using TradeFlowAI.Domain.Exceptions;

namespace TradeFlowAI.UnitTests.Domain;

public class WalletTests
{
    [Fact]
    public void Deposit_ValidAmount_ShouldIncreaseBalanceAndRecordTransaction()
    {
        // Arrange
        var wallet = Wallet.Create(Guid.NewGuid(), Guid.NewGuid(), "USD");

        // Act
        wallet.Deposit(5000m, "Initial deposit");

        // Assert
        wallet.Balance.Amount.Should().Be(5000m);
        wallet.AvailableBalance.Amount.Should().Be(5000m);
        wallet.Transactions.Should().HaveCount(1);
        wallet.DomainEvents.Should().HaveCount(1);
    }

    [Fact]
    public void Withdraw_SufficientBalance_ShouldDecreaseBalance()
    {
        // Arrange
        var wallet = Wallet.Create(Guid.NewGuid(), Guid.NewGuid(), "USD");
        wallet.Deposit(1000m, "Deposit");

        // Act
        wallet.Withdraw(400m, "Withdrawal");

        // Assert
        wallet.Balance.Amount.Should().Be(600m);
        wallet.AvailableBalance.Amount.Should().Be(600m);
        wallet.Transactions.Should().HaveCount(2);
    }

    [Fact]
    public void Withdraw_InsufficientBalance_ShouldThrowInsufficientFundsException()
    {
        // Arrange
        var wallet = Wallet.Create(Guid.NewGuid(), Guid.NewGuid(), "USD");
        wallet.Deposit(100m, "Deposit");

        // Act
        var act = () => wallet.Withdraw(500m, "Overdraw");

        // Assert
        act.Should().Throw<InsufficientFundsException>();
    }

    [Fact]
    public void Reserve_ValidAmount_ShouldDecreaseAvailableBalance()
    {
        // Arrange
        var wallet = Wallet.Create(Guid.NewGuid(), Guid.NewGuid(), "USD");
        wallet.Deposit(1000m, "Deposit");

        // Act
        wallet.Reserve(300m);

        // Assert
        wallet.Balance.Amount.Should().Be(1000m);
        wallet.ReservedBalance.Amount.Should().Be(300m);
        wallet.AvailableBalance.Amount.Should().Be(700m);
    }
}
