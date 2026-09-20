using Mizan.Domain.Finance;

namespace Mizan.Application.Finance;

public readonly record struct MoneyInput(long MinorUnits, string Currency)
{
    public static MoneyInput FromMinorUnits(long minorUnits, string currency) => new(minorUnits, currency);
}

public sealed record AcceptIncomeCommand(Guid AccountId, MoneyInput Amount, string EffectiveAt, string IdempotencyKey);
public sealed record AcceptExpenseCommand(Guid AccountId, MoneyInput Amount, string EffectiveAt, string IdempotencyKey);
public sealed record TransferCommand(Guid SourceAccountId, Guid DestinationAccountId, MoneyInput Amount, string EffectiveAt, string IdempotencyKey);
public sealed record ReverseOperationCommand(Guid OriginalOperationId, string EffectiveAt, string IdempotencyKey);
public sealed record AcceptRecoverableExpenseCommand(Guid AccountId, MoneyInput Amount, string CounterpartyName, string EffectiveAt, string IdempotencyKey);
public sealed record SettleRecoverableCommand(Guid AccountId, Guid RecoverableId, MoneyInput Amount, string EffectiveAt, string IdempotencyKey);
public sealed record CloseAccountCommand(Guid AccountId);
public sealed record ReopenAccountCommand(Guid AccountId);
