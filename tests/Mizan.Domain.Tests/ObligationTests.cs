using FluentAssertions;
using Mizan.Domain.Finance;
using Xunit;

namespace Mizan.Domain.Tests;

public sealed class ObligationTests
{
    [Fact]
    public void New_obligation_should_be_planned()
    {
        var obligation = Obligation.Create("Rent", Money.FromMinorUnits(5000, "EGP"), DateTimeOffset.Parse("2026-10-01T00:00:00+00:00"));

        obligation.Status.Should().Be(ObligationStatus.Planned);
        obligation.Amount.MinorUnits.Should().Be(5000);
    }

    [Fact]
    public void Planned_obligation_should_settle()
    {
        var obligation = Obligation.Create("Rent", Money.FromMinorUnits(5000, "EGP"), DateTimeOffset.UtcNow);

        obligation.Settle();

        obligation.Status.Should().Be(ObligationStatus.Settled);
    }

    [Fact]
    public void Planned_obligation_should_cancel()
    {
        var obligation = Obligation.Create("Rent", Money.FromMinorUnits(5000, "EGP"), DateTimeOffset.UtcNow);

        obligation.Cancel();

        obligation.Status.Should().Be(ObligationStatus.Cancelled);
    }

    [Fact]
    public void Settled_obligation_should_not_be_cancelled()
    {
        var obligation = Obligation.Create("Rent", Money.FromMinorUnits(5000, "EGP"), DateTimeOffset.UtcNow);
        obligation.Settle();

        var action = () => obligation.Cancel();

        action.Should().Throw<DomainValidationException>();
    }

    [Fact]
    public void Obligation_amount_must_be_positive()
    {
        var action = () => Obligation.Create("Rent", Money.FromMinorUnits(0, "EGP"), DateTimeOffset.UtcNow);

        action.Should().Throw<DomainValidationException>();
    }
}
