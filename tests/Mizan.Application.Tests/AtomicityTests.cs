using FluentAssertions;
using Mizan.Application.Finance;

namespace Mizan.Application.Tests;

public sealed class AtomicityTests
{
    [Fact]
    public async Task Rejected_transfer_should_leave_no_partial_effects()
    {
        var service = TestFinanceApplication.CreateInMemory();
        var source = await service.CreateAccountAsync("Cash", AccountType.Cash, "EGP");
        var destination = await service.CreateAccountAsync("Bank", AccountType.Bank, "EGP");

        var before = await service.CountEffectsAsync(source.Id) + await service.CountEffectsAsync(destination.Id);

        var action = () => service.TransferAsync(new TransferCommand(
            source.Id,
            destination.Id,
            MoneyInput.FromMinorUnits(100_00, "USD"),
            "2026-09-20T10:00:00+03:00",
            "idem-atomic-001"));

        await action.Should().ThrowAsync<DomainValidationException>();

        var after = await service.CountEffectsAsync(source.Id) + await service.CountEffectsAsync(destination.Id);
        after.Should().Be(before);
    }
}
