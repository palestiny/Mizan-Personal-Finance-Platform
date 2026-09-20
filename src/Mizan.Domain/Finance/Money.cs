namespace Mizan.Domain.Finance;

public readonly record struct Money(long MinorUnits, string Currency)
{
    public static Money FromMinorUnits(long minorUnits, string currency)
    {
        if (string.IsNullOrWhiteSpace(currency))
            throw new DomainValidationException("Currency is required.");

        return new Money(minorUnits, currency.Trim().ToUpperInvariant());
    }

    public static Money operator +(Money left, Money right)
    {
        EnsureSameCurrency(left, right);
        return new Money(checked(left.MinorUnits + right.MinorUnits), left.Currency);
    }

    public static Money operator -(Money left, Money right)
    {
        EnsureSameCurrency(left, right);
        return new Money(checked(left.MinorUnits - right.MinorUnits), left.Currency);
    }

    private static void EnsureSameCurrency(Money left, Money right)
    {
        if (!string.Equals(left.Currency, right.Currency, StringComparison.Ordinal))
            throw new DomainValidationException("Currencies must match.");
    }
}
