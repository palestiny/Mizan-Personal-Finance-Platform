namespace Mizan.Domain.Finance;

public enum RecoverableEffectDirection
{
    Increase,
    Decrease
}

public sealed class RecoverableEffect
{
    internal RecoverableEffect(Guid id, Guid operationId, Guid recoverableId, Money amount, RecoverableEffectDirection direction, string? counterpartyName, DateTimeOffset effectiveAt, DateTimeOffset recordedAt, long order)
    {
        Id = id;
        OperationId = operationId;
        RecoverableId = recoverableId;
        Amount = amount;
        Direction = direction;
        CounterpartyName = counterpartyName;
        EffectiveAt = effectiveAt;
        RecordedAt = recordedAt;
        Order = order;
    }

    public Guid Id { get; }
    public Guid OperationId { get; }
    public Guid RecoverableId { get; }
    public Money Amount { get; }
    public RecoverableEffectDirection Direction { get; }
    public string? CounterpartyName { get; }
    public DateTimeOffset EffectiveAt { get; }
    public DateTimeOffset RecordedAt { get; }
    public long Order { get; }

    public long SignedMinorUnits => Direction == RecoverableEffectDirection.Increase ? Amount.MinorUnits : -Amount.MinorUnits;

    public static RecoverableEffect Rehydrate(Guid id, Guid operationId, Guid recoverableId, Money amount, RecoverableEffectDirection direction, string? counterpartyName, DateTimeOffset effectiveAt, DateTimeOffset recordedAt, long order) =>
        new(id, operationId, recoverableId, amount, direction, counterpartyName, effectiveAt, recordedAt, order);
}
