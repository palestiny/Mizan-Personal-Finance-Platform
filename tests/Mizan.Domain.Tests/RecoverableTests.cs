using FluentAssertions;
using Mizan.Domain.Finance;
using Xunit;

namespace Mizan.Domain.Tests;

public sealed class RecoverableTests
{
    [Fact]
    public void Recoverable_expense_should_create_account_decrease_and_claim_increase()
    {
        var operation = FinancialOperation.RecoverableExpense(Guid.NewGuid(), Money.FromMinorUnits(1000, "EGP"), "Ahmed", DateTimeOffset.Parse("2026-09-21T10:00:00+03:00")).Accept();

        operation.Type.Should().Be(FinancialOperationType.RecoverableExpense);
        operation.Effects.Should().ContainSingle().Which.Direction.Should().Be(EffectDirection.Decrease);
        operation.RecoverableEffects.Should().ContainSingle().Which.Direction.Should().Be(RecoverableEffectDirection.Increase);
        operation.RecoverableEffects[0].Amount.MinorUnits.Should().Be(1000);
        operation.RecoverableEffects[0].CounterpartyName.Should().Be("Ahmed");
    }

    [Fact]
    public void Recoverable_settlement_should_create_claim_decrease()
    {
        var recoverableId = Guid.NewGuid();
        var operation = FinancialOperation.RecoverableSettlement(Guid.NewGuid(), recoverableId, Money.FromMinorUnits(400, "EGP"), DateTimeOffset.Parse("2026-09-22T10:00:00+03:00")).Accept();

        operation.Type.Should().Be(FinancialOperationType.RecoverableSettlement);
        operation.Effects.Should().ContainSingle().Which.Direction.Should().Be(EffectDirection.Increase);
        operation.RecoverableEffects.Should().ContainSingle().Which.Direction.Should().Be(RecoverableEffectDirection.Decrease);
        operation.RecoverableEffects[0].RecoverableId.Should().Be(recoverableId);
    }

    [Fact]
    public void Reversal_should_inverse_both_account_and_recoverable_effects()
    {
        var original = FinancialOperation.RecoverableExpense(Guid.NewGuid(), Money.FromMinorUnits(1000, "EGP"), "Ahmed", DateTimeOffset.Parse("2026-09-21T10:00:00+03:00")).Accept();
        var reversal = FinancialOperation.Reversal(original, DateTimeOffset.Parse("2026-09-23T10:00:00+03:00")).Accept();

        reversal.Effects.Should().ContainSingle().Which.Direction.Should().Be(EffectDirection.Increase);
        reversal.RecoverableEffects.Should().ContainSingle().Which.Direction.Should().Be(RecoverableEffectDirection.Decrease);
        reversal.RecoverableEffects[0].RecoverableId.Should().Be(original.RecoverableEffects[0].RecoverableId);
        reversal.OriginalOperationId.Should().Be(original.Id);
    }
}
