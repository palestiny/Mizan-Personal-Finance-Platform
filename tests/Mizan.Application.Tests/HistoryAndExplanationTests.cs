using FluentAssertions;
using Mizan.Application.Finance;

namespace Mizan.Application.Tests;

public sealed class HistoryAndExplanationTests
{
    [Fact]
    public async Task History_should_have_deterministic_order_and_balance_explanation_should_use_effects()
    {
        var service = TestFinanceApplication.CreateInMemory();
        var account = await service.CreateAccountAsync("Cash", AccountType.Cash, "EGP");

        await service.AcceptIncomeAsync(new AcceptIncomeCommand(
            account.Id, MoneyInput.FromMinorUnits(100_00, "EGP"),
            "2026-09-20T10:00:00+03:00", "idem-h-1"));

        await service.AcceptExpenseAsync(new AcceptExpenseCommand(
            account.Id, MoneyInput.FromMinorUnits(25_00, "EGP"),
            "2026-09-20T11:00:00+03:00", "idem-h-2"));

        var history = await service.GetHistoryAsync(account.Id);
        var explanation = await service.ExplainBalanceAsync(account.Id);

        history.Should().BeInAscendingOrderBy(x => x.EffectiveAt);
        explanation.Effects.Should().HaveCount(2);
        explanation.Balance.Should().Be(MoneyView.FromMinorUnits(75_00, "EGP"));
    }
}
