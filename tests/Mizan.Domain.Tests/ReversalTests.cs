using FluentAssertions;
using Mizan.Domain.Finance;

namespace Mizan.Domain.Tests;

public sealed class ReversalTests
{
    [Fact]
    public void Reversal_should_create_an_immutable_operation_linked_to_original_and_invert_each_effect()
    {
        var account = Account.Create("Cash", AccountType.Cash, "EGP");
        var original = FinancialOperation.Income(
            account.Id,
            Money.FromMinorUnits(1_000, "EGP"),
            DateTimeOffset.Parse("2026-09-20T10:00:00+03:00")).Accept();

        var reversal = FinancialOperation.Reversal(
            original,
            DateTimeOffset.Parse("2026-09-21T10:00:00+03:00")).Accept();

        reversal.Type.Should().Be(FinancialOperationType.Reversal);
        reversal.OriginalOperationId.Should().Be(original.Id);
        reversal.IsImmutable.Should().BeTrue();
        reversal.Effects.Should().HaveCount(original.Effects.Count);

        reversal.Effects[0].AccountId.Should().Be(original.Effects[0].AccountId);
        reversal.Effects[0].Amount.Should().Be(original.Effects[0].Amount);
        reversal.Effects[0].Direction.Should().Be(EffectDirection.Decrease);
        reversal.Effects[0].Order.Should().Be(original.Effects[0].Order);
        reversal.Effects[0].EffectiveAt.Should().Be(reversal.EffectiveAt);
    }

    [Fact]
    public void Reversal_of_transfer_should_invert_both_effects_and_preserve_conservation()
    {
        var source = Account.Create("Bank", AccountType.Bank, "EGP");
        var destination = Account.Create("Wallet", AccountType.Wallet, "EGP");
        var original = FinancialOperation.OwnedAccountTransfer(
            source.Id,
            destination.Id,
            Money.FromMinorUnits(500, "EGP"),
            DateTimeOffset.Parse("2026-09-20T10:00:00+03:00")).Accept();

        var reversal = FinancialOperation.Reversal(
            original,
            DateTimeOffset.Parse("2026-09-21T10:00:00+03:00")).Accept();

        reversal.Effects.Should().HaveCount(2);

        foreach (var originalEffect in original.Effects)
        {
            var inverse = reversal.Effects.Single(x => x.Order == originalEffect.Order);
            inverse.AccountId.Should().Be(originalEffect.AccountId);
            inverse.Amount.Should().Be(originalEffect.Amount);
            inverse.Direction.Should().NotBe(originalEffect.Direction);
        }

        reversal.Effects.Sum(x => x.SignedMinorUnits).Should().Be(0);
    }

    [Fact]
    public void Reversal_should_normalize_effective_timestamp_to_utc()
    {
        var account = Account.Create("Cash", AccountType.Cash, "EGP");
        var original = FinancialOperation.Income(
            account.Id,
            Money.FromMinorUnits(100, "EGP"),
            DateTimeOffset.Parse("2026-09-20T10:00:00+03:00")).Accept();

        var reversal = FinancialOperation.Reversal(
            original,
            DateTimeOffset.Parse("2026-09-21T10:00:00+03:00")).Accept();

        reversal.EffectiveAt.Offset.Should().Be(TimeSpan.Zero);
        reversal.Effects.Single().EffectiveAt.Offset.Should().Be(TimeSpan.Zero);
    }

    [Fact]
    public void Reversal_of_a_reversal_should_be_rejected()
    {
        var account = Account.Create("Cash", AccountType.Cash, "EGP");
        var original = FinancialOperation.Income(
            account.Id,
            Money.FromMinorUnits(100, "EGP"),
            DateTimeOffset.UtcNow).Accept();
        var reversal = FinancialOperation.Reversal(
            original,
            DateTimeOffset.UtcNow.AddMinutes(1)).Accept();

        Action act = () => FinancialOperation.Reversal(
            reversal,
            DateTimeOffset.UtcNow.AddMinutes(2));

        act.Should().Throw<DomainValidationException>();
    }
}
