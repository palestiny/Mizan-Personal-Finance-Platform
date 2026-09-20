using FluentAssertions;
using Mizan.Domain.Finance;

namespace Mizan.Domain.Tests;

public sealed class BalanceTests
{
    [Fact]
    public void Balance_should_be_derived_from_opening_state_and_effects()
    {
        var account = Account.Create("Cash", AccountType.Cash, "EGP", Money.FromMinorUnits(1_000_00, "EGP"));
        var income = FinancialOperation.Income(
            account.Id, Money.FromMinorUnits(250_00, "EGP"),
            DateTimeOffset.Parse("2026-09-20T10:00:00+03:00")).Accept();
        var expense = FinancialOperation.PersonalExpense(
            account.Id, Money.FromMinorUnits(75_00, "EGP"),
            DateTimeOffset.Parse("2026-09-20T11:00:00+03:00")).Accept();

        var balance = Balance.Derive(account, income.Effects.Concat(expense.Effects));

        balance.Should().Be(Money.FromMinorUnits(1_175_00, "EGP"));
    }

    [Fact]
    public void Rebuilding_balance_from_authoritative_effects_should_preserve_result()
    {
        var account = Account.Create("Cash", AccountType.Cash, "EGP", Money.FromMinorUnits(0, "EGP"));
        var effects = new[]
        {
            FinancialOperation.Income(account.Id, Money.FromMinorUnits(100_00, "EGP"), DateTimeOffset.UtcNow).Accept().Effects.Single(),
            FinancialOperation.PersonalExpense(account.Id, Money.FromMinorUnits(30_00, "EGP"), DateTimeOffset.UtcNow.AddMinutes(1)).Accept().Effects.Single()
        };

        var first = Balance.Derive(account, effects);
        var rebuilt = Balance.Rebuild(account, effects);

        rebuilt.Should().Be(first);
    }
}
