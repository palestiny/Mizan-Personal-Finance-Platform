using FluentAssertions;
using Mizan.Domain.Finance;

namespace Mizan.Domain.Tests;

public sealed class OperationTests
{
    [Fact]
    public void Income_should_create_one_positive_effect_on_receiving_account()
    {
        var account = Account.Create("Cash", AccountType.Cash, "EGP");
        var command = FinancialOperation.Income(
            account.Id,
            Money.FromMinorUnits(100_00, "EGP"),
            DateTimeOffset.Parse("2026-09-20T10:00:00+03:00"));

        var operation = command.Accept();

        operation.Effects.Should().ContainSingle();
        operation.Effects.Single().AccountId.Should().Be(account.Id);
        operation.Effects.Single().Direction.Should().Be(EffectDirection.Increase);
        operation.Effects.Single().Amount.Should().Be(Money.FromMinorUnits(100_00, "EGP"));
    }

    [Fact]
    public void Personal_expense_should_create_one_negative_effect_on_spending_account()
    {
        var account = Account.Create("Wallet", AccountType.Wallet, "EGP");
        var operation = FinancialOperation.PersonalExpense(
            account.Id,
            Money.FromMinorUnits(25_50, "EGP"),
            DateTimeOffset.Parse("2026-09-20T11:00:00+03:00")).Accept();

        operation.Effects.Should().ContainSingle();
        operation.Effects.Single().Direction.Should().Be(EffectDirection.Decrease);
    }

    [Fact]
    public void Transfer_should_create_two_conserving_effects()
    {
        var source = Account.Create("Cash", AccountType.Cash, "EGP");
        var destination = Account.Create("Bank", AccountType.Bank, "EGP");

        var operation = FinancialOperation.OwnedAccountTransfer(
            source.Id,
            destination.Id,
            Money.FromMinorUnits(500_00, "EGP"),
            DateTimeOffset.Parse("2026-09-20T12:00:00+03:00")).Accept();

        operation.Effects.Should().HaveCount(2);
        operation.Effects.Where(x => x.Direction == EffectDirection.Decrease)
            .Single().Amount.Should().Be(Money.FromMinorUnits(500_00, "EGP"));
        operation.Effects.Where(x => x.Direction == EffectDirection.Increase)
            .Single().Amount.Should().Be(Money.FromMinorUnits(500_00, "EGP"));
    }

    [Fact]
    public void Transfer_should_reject_same_source_and_destination()
    {
        var account = Account.Create("Cash", AccountType.Cash, "EGP");

        var action = () => FinancialOperation.OwnedAccountTransfer(
            account.Id,
            account.Id,
            Money.FromMinorUnits(100_00, "EGP"),
            DateTimeOffset.UtcNow).Accept();

        action.Should().Throw<DomainValidationException>();
    }
}
