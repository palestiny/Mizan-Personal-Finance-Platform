namespace Mizan.Domain.Finance;

public enum FinancialOperationType
{
    Income,
    PersonalExpense,
    OwnedAccountTransfer
}

public sealed class FinancialOperation
{
    private FinancialOperation(Guid id, FinancialOperationType type, DateTimeOffset effectiveAt, DateTimeOffset recordedAt, IReadOnlyList<FinancialEffect> effects)
    {
        Id = id;
        Type = type;
        EffectiveAt = effectiveAt;
        RecordedAt = recordedAt;
        Effects = effects;
        IsImmutable = true;
    }

    public Guid Id { get; }
    public FinancialOperationType Type { get; }
    public DateTimeOffset EffectiveAt { get; }
    public DateTimeOffset RecordedAt { get; }
    public IReadOnlyList<FinancialEffect> Effects { get; }
    public bool IsImmutable { get; }

    public static FinancialOperation Rehydrate(Guid id, FinancialOperationType type, DateTimeOffset effectiveAt, DateTimeOffset recordedAt, IReadOnlyList<FinancialEffect> effects) =>
        new(id, type, effectiveAt, recordedAt, effects);

    public static OperationBuilder Income(Guid accountId, Money amount, DateTimeOffset effectiveAt) =>
        new(FinancialOperationType.Income, effectiveAt, (id, recordedAt) =>
            new FinancialOperation(id, FinancialOperationType.Income, effectiveAt, recordedAt,
                new[] { CreateEffect(id, accountId, amount, EffectDirection.Increase, effectiveAt, recordedAt, 0) }));

    public static OperationBuilder PersonalExpense(Guid accountId, Money amount, DateTimeOffset effectiveAt) =>
        new(FinancialOperationType.PersonalExpense, effectiveAt, (id, recordedAt) =>
            new FinancialOperation(id, FinancialOperationType.PersonalExpense, effectiveAt, recordedAt,
                new[] { CreateEffect(id, accountId, amount, EffectDirection.Decrease, effectiveAt, recordedAt, 0) }));

    public static OperationBuilder OwnedAccountTransfer(Guid sourceAccountId, Guid destinationAccountId, Money amount, DateTimeOffset effectiveAt)
    {
        if (sourceAccountId == destinationAccountId)
            throw new DomainValidationException("Transfer source and destination must differ.");

        return new OperationBuilder(FinancialOperationType.OwnedAccountTransfer, effectiveAt, (id, recordedAt) =>
            new FinancialOperation(id, FinancialOperationType.OwnedAccountTransfer, effectiveAt, recordedAt,
                new[]
                {
                    CreateEffect(id, sourceAccountId, amount, EffectDirection.Decrease, effectiveAt, recordedAt, 0),
                    CreateEffect(id, destinationAccountId, amount, EffectDirection.Increase, effectiveAt, recordedAt, 1)
                }));
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
