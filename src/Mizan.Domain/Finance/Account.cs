namespace Mizan.Domain.Finance;

public enum AccountType
{
    Cash,
    Bank,
    Wallet
}

public sealed class Account
{
    private Account(Guid id, string name, AccountType type, string currency, Money openingBalance)
    {
        Id = id;
        Name = name;
        Type = type;
        Currency = currency;
        OpeningBalance = openingBalance;
    }

    public Guid Id { get; }
    public string Name { get; }
    public AccountType Type { get; }
    public string Currency { get; }
    public Money OpeningBalance { get; }

    public static Account Create(string name, AccountType type, string currency, Money? openingBalance = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainValidationException("Account name is required.");

        var opening = openingBalance ?? Money.FromMinorUnits(0, currency);
        if (!string.Equals(opening.Currency, currency, StringComparison.OrdinalIgnoreCase))
            throw new DomainValidationException("Opening balance currency must match account currency.");

        return new Account(Guid.NewGuid(), name.Trim(), type, currency.Trim().ToUpperInvariant(), opening);
    }
}
