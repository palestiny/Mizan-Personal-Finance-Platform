using System.Collections.Immutable;

namespace Mizan.Domain.Finance;

public enum FinancialOperationType
{
    Income,
    PersonalExpense,
    OwnedAccountTransfer,
    Reversal
}

public sealed class FinancialOperation
{
    private FinancialOperation(Guid id, FinancialOperationType type, DateTimeOffset effectiveAt, DateTimeOffset recordedAt, IReadOnlyList<FinancialEffect> effects, Guid? originalOperationId = null)
    {
        Id = id;
        Type = type;
        EffectiveAt = effectiveAt;
        RecordedAt = recordedAt;
        OriginalOperationId = originalOperationId;
        Effects = effects;
        IsImmutable = true;
    }

    public Guid Id { get; }
    public FinancialOperationType Type { get; }
    public DateTimeOffset EffectiveAt { get; }
    public DateTimeOffset RecordedAt { get; }
    public Guid? OriginalOperationId { get; }
    public IReadOnlyList<FinancialEffect> Effects { get; }
    public bool IsImmutable { get; }

    public static FinancialOperation Rehydrate(Guid id, FinancialOperationType type, DateTimeOffset effectiveAt, DateTimeOffset recordedAt, IReadOnlyList<FinancialEffect> effects, Guid? originalOperationId = null) =>
        new(id, type, effectiveAt, recordedAt, effects.ToImmutableArray(), originalOperationId);

    public static OperationBuilder Income(Guid accountId, Money amount, DateTimeOffset effectiveAt)
    {
        var normalizedEffectiveAt = effectiveAt.ToUniversalTime();
        return new(FinancialOperationType.Income, normalizedEffectiveAt, (id, recordedAt) =>
            new FinancialOperation(id, FinancialOperationType.Income, normalizedEffectiveAt, recordedAt,
                ImmutableArray.Create(CreateEffect(id, accountId, amount, EffectDirection.Increase, normalizedEffectiveAt, recordedAt, 0))));
    }

    public static OperationBuilder PersonalExpense(Guid accountId, Money amount, DateTimeOffset effectiveAt)
    {
        var normalizedEffectiveAt = effectiveAt.ToUniversalTime();
        return new(FinancialOperationType.PersonalExpense, normalizedEffectiveAt, (id, recordedAt) =>
            new FinancialOperation(id, FinancialOperationType.PersonalExpense, normalizedEffectiveAt, recordedAt,
                ImmutableArray.Create(CreateEffect(id, accountId, amount, EffectDirection.Decrease, normalizedEffectiveAt, recordedAt, 0))));
    }

    public static OperationBuilder Reversal(FinancialOperation original, DateTimeOffset effectiveAt)
    {
        if (original.Type == FinancialOperationType.Reversal)
            throw new DomainValidationException("A reversal cannot target another reversal.");

        var normalizedEffectiveAt = effectiveAt.ToUniversalTime();
        return new OperationBuilder(FinancialOperationType.Reversal, normalizedEffectiveAt, (id, recordedAt) =>
            new FinancialOperation(
                id,
                FinancialOperationType.Reversal,
                normalizedEffectiveAt,
                recordedAt,
                original.Effects
                    .OrderBy(x => x.Order)
                    .Select(x => CreateEffect(
                        id,
                        x.AccountId,
                        x.Amount,
                        x.Direction == EffectDirection.Increase ? EffectDirection.Decrease : EffectDirection.Increase,
                        normalizedEffectiveAt,
                        recordedAt,
                        x.Order))
                    .ToImmutableArray(),
                original.Id));
    }

    public static OperationBuilder OwnedAccountTransfer(Guid sourceAccountId, Guid destinationAccountId, Money amount, DateTimeOffset effectiveAt)
    {
        if (sourceAccountId == destinationAccountId)
            throw new DomainValidationException("Transfer source and destination must differ.");

        var normalizedEffectiveAt = effectiveAt.ToUniversalTime();
        return new OperationBuilder(FinancialOperationType.OwnedAccountTransfer, normalizedEffectiveAt, (id, recordedAt) =>
            new FinancialOperation(id, FinancialOperationType.OwnedAccountTransfer, normalizedEffectiveAt, recordedAt,
                ImmutableArray.Create(
                    CreateEffect(id, sourceAccountId, amount, EffectDirection.Decrease, normalizedEffectiveAt, recordedAt, 0),
                    CreateEffect(id, destinationAccountId, amount, EffectDirection.Increase, normalizedEffectiveAt, recordedAt, 1))));
    }

    private static FinancialEffect CreateEffect(Guid operationId, Guid accountId, Money amount, EffectDirection direction, DateTimeOffset effectiveAt, DateTimeOffset recordedAt, long order)
    {
        if (amount.MinorUnits <= 0)
            throw new DomainValidationException("Financial amount must be greater than zero.");

        return new FinancialEffect(Guid.NewGuid(), operationId, accountId, amount, direction, effectiveAt, recordedAt, order);
    }

    public sealed class OperationBuilder
    {
        private readonly Func<Guid, DateTimeOffset, FinancialOperation> _factory;

        internal OperationBuilder(FinancialOperationType type, DateTimeOffset effectiveAt, Func<Guid, DateTimeOffset, FinancialOperation> factory)
        {
            Type = type;
            EffectiveAt = effectiveAt;
            _factory = factory;
        }

        public FinancialOperationType Type { get; }
        public DateTimeOffset EffectiveAt { get; }

        public FinancialOperation Accept() => _factory(Guid.NewGuid(), DateTimeOffset.UtcNow);
    }
}
