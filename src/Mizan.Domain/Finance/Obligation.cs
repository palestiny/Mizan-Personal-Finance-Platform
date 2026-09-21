namespace Mizan.Domain.Finance;

public enum ObligationStatus
{
    Planned,
    Settled,
    Cancelled
}

public sealed class Obligation
{
    private Obligation(Guid id, string description, Money amount, DateTimeOffset dueAt, DateTimeOffset createdAt, ObligationStatus status)
    {
        Id = id;
        Description = description;
        Amount = amount;
        DueAt = dueAt;
        CreatedAt = createdAt;
        Status = status;
    }

    public Guid Id { get; }
    public string Description { get; }
    public Money Amount { get; }
    public DateTimeOffset DueAt { get; }
    public DateTimeOffset CreatedAt { get; }
    public ObligationStatus Status { get; private set; }

    public static Obligation Create(string description, Money amount, DateTimeOffset dueAt)
    {
        if (string.IsNullOrWhiteSpace(description))
            throw new DomainValidationException("Obligation description is required.");
        if (amount.MinorUnits <= 0)
            throw new DomainValidationException("Obligation amount must be greater than zero.");

        return new Obligation(Guid.NewGuid(), description.Trim(), amount, dueAt.ToUniversalTime(), DateTimeOffset.UtcNow, ObligationStatus.Planned);
    }

    public static Obligation Rehydrate(Guid id, string description, Money amount, DateTimeOffset dueAt, DateTimeOffset createdAt, ObligationStatus status) =>
        new(id, description, amount, dueAt, createdAt, status);

    public void Settle()
    {
        if (Status != ObligationStatus.Planned)
            throw new DomainValidationException("Only planned obligations can be settled.");
        Status = ObligationStatus.Settled;
    }

    public void Cancel()
    {
        if (Status != ObligationStatus.Planned)
            throw new DomainValidationException("Only planned obligations can be cancelled.");
        Status = ObligationStatus.Cancelled;
    }
}
