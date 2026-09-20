using FluentAssertions;
using Mizan.Domain.Finance;
using Xunit;

namespace Mizan.Domain.Tests;

public sealed class SharedExpenseTests
{
    [Fact]
    public void Shared_expense_should_decrease_account_by_total_and_create_recoverable_for_share()
    {
        var operation = FinancialOperation.SharedExpense(
            Guid.NewGuid(),
            Money.FromMinorUnits(1000, "EGP"),
            Money.FromMinorUnits(400, "EGP"),
            "Ahmed",
            new DateTimeOffset(2026, 9, 21, 10, 0, 0, TimeSpan.FromHours(3))).Accept();

        operation.Type.Should().Be(FinancialOperationType.SharedExpense);
        operation.Effects.Should().ContainSingle();
        operation.Effects[0].Amount.MinorUnits.Should().Be(1000);
        operation.Effects[0].Direction.Should().Be(EffectDirection.Decrease);
        operation.RecoverableEffects.Should().ContainSingle();
        operation.RecoverableEffects[0].Amount.MinorUnits.Should().Be(400);
        operation.RecoverableEffects[0].Direction.Should().Be(RecoverableEffectDirection.Increase);
        operation.RecoverableEffects[0].CounterpartyName.Should().Be("Ahmed");
    }

    [Fact]
    public void Shared_expense_should_reject_zero_recoverable_amount()
    {
        var action = () => FinancialOperation.SharedExpense(
            Guid.NewGuid(),
            Money.FromMinorUnits(1000, "EGP"),
            Money.FromMinorUnits(0, "EGP"),
            "Ahmed",
            DateTimeOffset.UtcNow);

        action.Should().Throw<DomainValidationException>();
    }

    [Fact]
    public void Shared_expense_should_reject_recoverable_amount_above_total()
    {
        var action = () => FinancialOperation.SharedExpense(
            Guid.NewGuid(),
            Money.FromMinorUnits(1000, "EGP"),
            Money.FromMinorUnits(1001, "EGP"),
            "Ahmed",
            DateTimeOffset.UtcNow);

        action.Should().Throw<DomainValidationException>();
    }

    [Fact]
    public void Shared_expense_should_reject_currency_mismatch()
    {
        var action = () => FinancialOperation.SharedExpense(
            Guid.NewGuid(),
            Money.FromMinorUnits(1000, "EGP"),
            Money.FromMinorUnits(400, "USD"),
            "Ahmed",
            DateTimeOffset.UtcNow);

        action.Should().Throw<DomainValidationException>();
    }

    [Fact]
    public void Reversing_shared_expense_should_inverse_both_effect_sets()
    {
        var original = FinancialOperation.SharedExpense(
            Guid.NewGuid(),
            Money.FromMinorUnits(1000, "EGP"),
            Money.FromMinorUnits(400, "EGP"),
            "Ahmed",
            DateTimeOffset.UtcNow).Accept();

        var reversal = FinancialOperation.Reversal(original, DateTimeOffset.UtcNow).Accept();

        reversal.Effects.Should().ContainSingle();
        reversal.Effects[0].Direction.Should().Be(EffectDirection.Increase);
        reversal.Effects[0].Amount.MinorUnits.Should().Be(1000);
        reversal.RecoverableEffects.Should().ContainSingle();
        reversal.RecoverableEffects[0].Direction.Should().Be(RecoverableEffectDirection.Decrease);
        reversal.RecoverableEffects[0].Amount.MinorUnits.Should().Be(400);
        reversal.OriginalOperationId.Should().Be(original.Id);
    }
}
