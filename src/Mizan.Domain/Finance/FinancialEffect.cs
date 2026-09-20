namespace Mizan.Domain.Finance;

public enum EffectDirection
{
    Increase,
    Decrease
}

public sealed class FinancialEffect
{
    internal FinancialEffect(Guid id, Guid operationId, Guid accountId, Money amount, EffectDirection direction, DateTimeOffset effectiveAt, DateTimeOffset recordedAt, long order)
    {
        Id = id;
        OperationId = operationId;
        AccountId = accountId;
        Amount = amount;
        Direction = direction;
        EffectiveAt = effectiveAt;
        RecordedAt = recordedAt;
        Order = order;
    }

    public Guid Id { get; }
    public Guid OperationId { get; }
    public Guid AccountId { get; }
    public Money Amount { get; }
    public EffectDirection Direction { get; }
    public DateTimeOffset EffectiveAt { get; }
    public DateTimeOffset RecordedAt { get; }
    public long Order { get; }

    public long SignedMinorUnits => Direction == EffectDirection.Increase ? Amount.MinorUnits : -Amount.MinorUnits;
}
