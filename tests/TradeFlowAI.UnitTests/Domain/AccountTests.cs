using FluentAssertions;
using TradeFlowAI.Domain.Aggregates.Account;
using TradeFlowAI.Domain.Enums;
using TradeFlowAI.Domain.Exceptions;
using TradeFlowAI.Domain.ValueObjects;

namespace TradeFlowAI.UnitTests.Domain;

public class AccountTests
{
    [Fact]
    public void Create_ValidParameters_ShouldInitializeWithPendingKycAndRaiseEvent()
    {
        // Arrange
        var id = Guid.NewGuid();
        var userId = "user-123";
        var name = "Trading Alpha";

        // Act
        var account = Account.Create(id, userId, name);

        // Assert
        account.Id.Should().Be(id);
        account.UserId.Should().Be(userId);
        account.Name.Should().Be(name);
        account.Status.Should().Be(AccountStatus.PendingKyc);
        account.IsAutoTradingEnabled.Should().BeFalse();
        account.DomainEvents.Should().HaveCount(1);
    }

    [Fact]
    public void Activate_WhenPendingKyc_ShouldChangeStatusToActive()
    {
        // Arrange
        var account = Account.Create(Guid.NewGuid(), "u1", "Acc 1");

        // Act
        account.Activate();

        // Assert
        account.Status.Should().Be(AccountStatus.Active);
    }

    [Fact]
    public void EnableAutoTrading_WhenActive_ShouldSetStrategyAndFlag()
    {
        // Arrange
        var account = Account.Create(Guid.NewGuid(), "u1", "Acc 1");
        account.Activate();
        var strategyId = Guid.NewGuid();

        // Act
        account.EnableAutoTrading(strategyId);

        // Assert
        account.IsAutoTradingEnabled.Should().BeTrue();
        account.ActiveStrategyId.Should().Be(strategyId);
    }

    [Fact]
    public void EnableAutoTrading_WhenPendingKyc_ShouldThrowDomainException()
    {
        // Arrange
        var account = Account.Create(Guid.NewGuid(), "u1", "Acc 1");
        var strategyId = Guid.NewGuid();

        // Act
        var act = () => account.EnableAutoTrading(strategyId);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("*not active*");
    }

    [Fact]
    public void UpdateRiskProfile_WhenActive_ShouldUpdateCorrectly()
    {
        // Arrange
        var account = Account.Create(Guid.NewGuid(), "u1", "Acc 1");
        account.Activate();
        var newProfile = RiskProfile.Aggressive();

        // Act
        account.UpdateRiskProfile(newProfile);

        // Assert
        account.RiskProfile.Should().Be(newProfile);
    }
}
