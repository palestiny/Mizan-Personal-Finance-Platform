using FluentAssertions;
using Mizan.Domain.Finance;
using Xunit;

namespace Mizan.Domain.Tests;

public sealed class AccountLifecycleTests
{
    [Fact]
    public void New_account_should_be_active()
    {
        var account = Account.Create("Cash", AccountType.Cash, "EGP");

        account.Status.Should().Be(AccountStatus.Active);
    }

    [Fact]
    public void Active_account_should_close()
    {
        var account = Account.Create("Cash", AccountType.Cash, "EGP");

        account.Close();

        account.Status.Should().Be(AccountStatus.Closed);
    }

    [Fact]
    public void Closed_account_should_reopen()
    {
        var account = Account.Create("Cash", AccountType.Cash, "EGP");
        account.Close();

        account.Reopen();

        account.Status.Should().Be(AccountStatus.Active);
    }

    [Fact]
    public void Closing_an_already_closed_account_should_fail()
    {
        var account = Account.Create("Cash", AccountType.Cash, "EGP");
        account.Close();

        var action = () => account.Close();

        action.Should().Throw<DomainValidationException>();
    }

    [Fact]
    public void Reopening_an_active_account_should_fail()
    {
        var account = Account.Create("Cash", AccountType.Cash, "EGP");

        var action = () => account.Reopen();

        action.Should().Throw<DomainValidationException>();
    }
}
