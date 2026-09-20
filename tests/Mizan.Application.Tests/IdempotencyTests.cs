using FluentAssertions;
using Mizan.Application.Finance;
using Mizan.Domain.Finance;

namespace Mizan.Application.Tests;

public sealed class IdempotencyTests
{
    [Fact]
    public async Task Repeating_the_same_command_should_not_duplicate_financial_effects()
    {
        var service = TestFinanceApplication.CreateInMemory();
        var account = await service.CreateAccountAsync("Cash", AccountType.Cash, "EGP");

        var command = new AcceptIncomeCommand(
            account.Id,
            MoneyInput.FromMinorUnits(100_00, "EGP"),
            "2026-09-20T10:00:00+03:00",
            "idem-001");

        var first = await service.AcceptIncomeAsync(command);
        var retry = await service.AcceptIncomeAsync(command);

        retry.Id.Should().Be(first.Id);
        (await service.CountEffectsAsync(account.Id)).Should().Be(1);
    }

    [Fact]
    public async Task Reusing_an_idempotency_key_for_a_different_command_should_conflict()
    {
        var service = TestFinanceApplication.CreateInMemory();
        var account = await service.CreateAccountAsync("Cash", AccountType.Cash, "EGP");

        await service.AcceptIncomeAsync(new AcceptIncomeCommand(
            account.Id,
            MoneyInput.FromMinorUnits(100_00, "EGP"),
            "2026-09-20T10:00:00+03:00",
            "idem-conflict"));

        var act = async () => await service.AcceptIncomeAsync(new AcceptIncomeCommand(
            account.Id,
            MoneyInput.FromMinorUnits(110_00, "EGP"),
            "2026-09-20T10:00:00+03:00",
            "idem-conflict"));

        await act.Should().ThrowAsync<IdempotencyConflictException>();
        (await service.CountEffectsAsync(account.Id)).Should().Be(1);
    }
}
