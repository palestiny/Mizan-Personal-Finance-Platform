namespace Mizan.Domain.Finance;

public enum AccountType
{
    Cash,
    Bank,
    Wallet
}

public enum AccountStatus
{
    Active,
    Closed
}

public sealed class Account
{
    private Account(Guid id, string name, AccountType type, string currency, Money openingBalance, AccountStatus status)
    {
        Id = id;
        Name = name;
        Type = type;
        Currency = currency;
        OpeningBalance = openingBalance;
        Status = status;
    }

    public Guid Id { get; }
    public string Name { get; }
    public AccountType Type { get; }
    public string Currency { get; }
    public Money OpeningBalance { get; }
    public AccountStatus Status { get; private set; }

    public static Account Rehydrate(Guid id, string name, AccountType type, string currency, Money openingBalance, AccountStatus status = AccountStatus.Active) =>
        new(id, name, type, currency.Trim().ToUpperInvariant(), openingBalance, status);

    public static Account Create(string name, AccountType type, string currency, Money? openingBalance = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainValidationException("Account name is required.");

        var opening = openingBalance ?? Money.FromMinorUnits(0, currency);
        if (!string.Equals(opening.Currency, currency, StringComparison.OrdinalIgnoreCase))
            throw new DomainValidationException("Opening balance currency must match account currency.");

        return new Account(Guid.NewGuid(), name.Trim(), type, currency.Trim().ToUpperInvariant(), opening, AccountStatus.Active);
    }

    public void Close()
    {
        if (Status == AccountStatus.Closed)
            throw new DomainValidationException("Account is already closed.");

        Status = AccountStatus.Closed;
    }

    public void Reopen()
    {
        if (Status == AccountStatus.Active)
            throw new DomainValidationException("Account is already active.");

        Status = AccountStatus.Active;
    }
}
