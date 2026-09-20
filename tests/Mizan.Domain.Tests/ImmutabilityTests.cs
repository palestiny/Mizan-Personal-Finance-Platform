using FluentAssertions;
using Mizan.Domain.Finance;

namespace Mizan.Domain.Tests;

public sealed class ImmutabilityTests
{
    [Fact]
    public void Accepted_operation_effects_should_not_be_mutable()
    {
        var account = Account.Create("Cash", AccountType.Cash, "EGP");
        var operation = FinancialOperation.Income(
            account.Id,
            Money.FromMinorUnits(100_00, "EGP"),
            DateTimeOffset.UtcNow).Accept();

        operation.IsImmutable.Should().BeTrue();
        operation.Effects.Should().NotBeAssignableTo<IList<FinancialEffect>>();
    }
}
