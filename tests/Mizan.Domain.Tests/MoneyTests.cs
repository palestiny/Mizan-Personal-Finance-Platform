using FluentAssertions;
using Mizan.Domain.Finance;

namespace Mizan.Domain.Tests;

public sealed class MoneyTests
{
    [Fact]
    public void Money_should_store_minor_units_exactly()
    {
        var money = Money.FromMinorUnits(12345, "EGP");

        money.MinorUnits.Should().Be(12345);
        money.Currency.Should().Be("EGP");
    }

    [Fact]
    public void Money_should_not_use_floating_point_for_authoritative_value()
    {
        var money = Money.FromMinorUnits(999, "EGP");

        money.MinorUnits.Should().Be(999L);
    }
}
