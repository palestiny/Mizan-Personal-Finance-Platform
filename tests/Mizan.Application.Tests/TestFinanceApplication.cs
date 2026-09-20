using Mizan.Domain.Finance;

namespace Mizan.Application.Finance;

public sealed class TestFinanceApplication
{
    private readonly Dictionary<Guid, Account> _accounts = new();
    private readonly Dictionary<string, FinancialOperation> _idempotency = new(StringComparer.Ordinal);
    private readonly List<FinancialEffect> _effects = new();

    public static TestFinanceApplication CreateInMemory() => new();

    public Task<Account> CreateAccountAsync(string name, AccountType type, string currency, Money? openingBalance = null)
    {
        var account = Account.Create(name, type, currency, openingBalance);
        _accounts.Add(account.Id, account);
        return Task.FromResult(account);
    }

    public Task<FinancialOperation> AcceptIncomeAsync(AcceptIncomeCommand command)
    {
        if (_idempotency.TryGetValue(command.IdempotencyKey, out var existing))
            return Task.FromResult(existing);

        var account = GetAccount(command.AccountId);
        ValidateCurrency(account, command.Amount.Currency);
        var operation = FinancialOperation.Income(account.Id, Money.FromMinorUnits(command.Amount.MinorUnits, command.Amount.Currency), Parse(command.EffectiveAt)).Accept();
        Commit(command.IdempotencyKey, operation);
        return Task.FromResult(operation);
    }

    public Task<FinancialOperation> AcceptExpenseAsync(AcceptExpenseCommand command)
    {
        if (_idempotency.TryGetValue(command.IdempotencyKey, out var existing))
            return Task.FromResult(existing);

        var account = GetAccount(command.AccountId);
        ValidateCurrency(account, command.Amount.Currency);
        var operation = FinancialOperation.PersonalExpense(account.Id, Money.FromMinorUnits(command.Amount.MinorUnits, command.Amount.Currency), Parse(command.EffectiveAt)).Accept();
        Commit(command.IdempotencyKey, operation);
        return Task.FromResult(operation);
    }

    public Task<FinancialOperation> TransferAsync(TransferCommand command)
    {
        if (_idempotency.TryGetValue(command.IdempotencyKey, out var existing))
            return Task.FromResult(existing);

        var source = GetAccount(command.SourceAccountId);
        var destination = GetAccount(command.DestinationAccountId);
        ValidateCurrency(source, command.Amount.Currency);
        ValidateCurrency(destination, command.Amount.Currency);

        var operation = FinancialOperation.OwnedAccountTransfer(
            source.Id, destination.Id,
            Money.FromMinorUnits(command.Amount.MinorUnits, command.Amount.Currency),
            Parse(command.EffectiveAt)).Accept();

        Commit(command.IdempotencyKey, operation);
        return Task.FromResult(operation);
    }

    public Task<int> CountEffectsAsync(Guid accountId) =>
        Task.FromResult(_effects.Count(x => x.AccountId == accountId));

    public Task<IReadOnlyList<HistoryItem>> GetHistoryAsync(Guid accountId) =>
        Task.FromResult<IReadOnlyList<HistoryItem>>(
            _effects.Where(x => x.AccountId == accountId)
                .OrderBy(x => x.EffectiveAt)
                .ThenBy(x => x.RecordedAt)
                .ThenBy(x => x.Order)
                .ThenBy(x => x.Id)
                .Select(x => new HistoryItem(x.EffectiveAt, x))
                .ToList());

    public Task<BalanceExplanation> ExplainBalanceAsync(Guid accountId)
    {
        var account = GetAccount(accountId);
        var effects = _effects.Where(x => x.AccountId == accountId).ToList();
        return Task.FromResult(new BalanceExplanation(Balance.Derive(account, effects), effects));
    }

    private void Commit(string key, FinancialOperation operation)
    {
        _idempotency.Add(key, operation);
        _effects.AddRange(operation.Effects);
    }

    private Account GetAccount(Guid id) =>
        _accounts.TryGetValue(id, out var account)
            ? account
            : throw new DomainValidationException("Account was not found.");

    private static void ValidateCurrency(Account account, string currency)
    {
        if (!string.Equals(account.Currency, currency, StringComparison.OrdinalIgnoreCase))
            throw new DomainValidationException("Currency does not match account.");
    }

    private static DateTimeOffset Parse(string value) =>
        DateTimeOffset.Parse(value, System.Globalization.CultureInfo.InvariantCulture);

    public sealed record HistoryItem(DateTimeOffset EffectiveAt, FinancialEffect Effect);
    public sealed record BalanceExplanation(Money Balance, IReadOnlyList<FinancialEffect> Effects);
}

public readonly record struct MoneyView(long AmountMinorUnits, string Currency)
{
    public static MoneyView FromMinorUnits(long amountMinorUnits, string currency) => new(amountMinorUnits, currency);
}
