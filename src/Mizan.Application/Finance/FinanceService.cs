using Mizan.Domain.Finance;

namespace Mizan.Application.Finance;

public sealed class FinanceService
{
    private readonly IFinanceRepository _repository;

    public FinanceService(IFinanceRepository repository) => _repository = repository;

    public async Task<Account> CreateAccountAsync(string name, AccountType type, string currency, Money? openingBalance, CancellationToken cancellationToken)
    {
        var account = Account.Create(name, type, currency, openingBalance);
        await _repository.AddAccountAsync(account, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);
        return account;
    }

    public async Task<Account> CloseAccountAsync(CloseAccountCommand command, CancellationToken cancellationToken)
    {
        var account = await _repository.GetAccountAsync(command.AccountId, cancellationToken)
            ?? throw new DomainValidationException("Account was not found.");
        account.Close();
        await _repository.UpdateAccountAsync(account, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);
        return account;
    }

    public async Task<Account> ReopenAccountAsync(ReopenAccountCommand command, CancellationToken cancellationToken)
    {
        var account = await _repository.GetAccountAsync(command.AccountId, cancellationToken)
            ?? throw new DomainValidationException("Account was not found.");
        account.Reopen();
        await _repository.UpdateAccountAsync(account, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);
        return account;
    }

    public async Task<FinancialOperation> AcceptAsync(FinancialOperation.OperationBuilder builder, string idempotencyKey, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(idempotencyKey))
            throw new DomainValidationException("Idempotency key is required.");

        var existing = await _repository.GetOperationByIdempotencyKeyAsync(idempotencyKey, cancellationToken);
        var operation = builder.Accept();

        if (existing is not null)
        {
            if (!SemanticallyMatches(existing, operation))
                throw new IdempotencyConflictException("The idempotency key is already associated with a different financial command.");

            return existing;
        }

        await ValidateActiveAccountsAsync(operation, cancellationToken);

        await _repository.BeginTransactionAsync(cancellationToken);
        try
        {
            await _repository.AddAcceptedOperationAsync(operation, idempotencyKey, cancellationToken);
            await _repository.SaveChangesAsync(cancellationToken);
            await _repository.CommitTransactionAsync(cancellationToken);
            return operation;
        }
        catch
        {
            await _repository.RollbackTransactionAsync(cancellationToken);

            var concurrent = await _repository.GetOperationByIdempotencyKeyAsync(idempotencyKey, cancellationToken);
            if (concurrent is not null)
            {
                if (!SemanticallyMatches(concurrent, operation))
                    throw new IdempotencyConflictException("The idempotency key is already associated with a different financial command.");

                return concurrent;
            }

            throw;
        }
    }

    public async Task<FinancialOperation> AcceptRecoverableExpenseAsync(AcceptRecoverableExpenseCommand command, CancellationToken cancellationToken) =>
        await AcceptAsync(
            FinancialOperation.RecoverableExpense(
                command.AccountId,
                Money.FromMinorUnits(command.Amount.MinorUnits, command.Amount.Currency),
                command.CounterpartyName,
                Parse(command.EffectiveAt)),
            command.IdempotencyKey,
            cancellationToken);

    public async Task<FinancialOperation> AcceptSharedExpenseAsync(AcceptSharedExpenseCommand command, CancellationToken cancellationToken) =>
        await AcceptAsync(
            FinancialOperation.SharedExpense(
                command.AccountId,
                Money.FromMinorUnits(command.TotalAmount.MinorUnits, command.TotalAmount.Currency),
                Money.FromMinorUnits(command.RecoverableAmount.MinorUnits, command.RecoverableAmount.Currency),
                command.CounterpartyName,
                Parse(command.EffectiveAt)),
            command.IdempotencyKey,
            cancellationToken);

    public async Task<FinancialOperation> AcceptRecoverableSettlementAsync(SettleRecoverableCommand command, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(command.IdempotencyKey))
            throw new DomainValidationException("Idempotency key is required.");

        var existing = await _repository.GetOperationByIdempotencyKeyAsync(command.IdempotencyKey, cancellationToken);
        var effectiveAt = Parse(command.EffectiveAt);
        var amount = Money.FromMinorUnits(command.Amount.MinorUnits, command.Amount.Currency);
        var candidate = FinancialOperation.RecoverableSettlement(command.AccountId, command.RecoverableId, amount, effectiveAt).Accept();

        if (existing is not null)
        {
            if (!SemanticallyMatches(existing, candidate))
                throw new IdempotencyConflictException("The idempotency key is already associated with a different financial command.");

            return existing;
        }

        Exception? lastFailure = null;

        for (var attempt = 1; attempt <= 3; attempt++)
        {
            await _repository.BeginTransactionAsync(cancellationToken, System.Data.IsolationLevel.Serializable);
            try
            {
                var recoverableEffects = await _repository.GetRecoverableEffectsAsync(command.RecoverableId, cancellationToken);
                if (recoverableEffects.Count == 0)
                    throw new DomainValidationException("Recoverable was not found.");

                var currency = recoverableEffects[0].Amount.Currency;
                if (!string.Equals(currency, amount.Currency, StringComparison.OrdinalIgnoreCase))
                    throw new DomainValidationException("Settlement currency must match recoverable currency.");

                var outstanding = checked(recoverableEffects.Sum(x => x.SignedMinorUnits));
                if (amount.MinorUnits > outstanding)
                    throw new DomainValidationException("Settlement amount cannot exceed the outstanding recoverable amount.");

                await ValidateActiveAccountsAsync(candidate, cancellationToken);
                await _repository.AddAcceptedOperationAsync(candidate, command.IdempotencyKey, cancellationToken);
                await _repository.SaveChangesAsync(cancellationToken);
                await _repository.CommitTransactionAsync(cancellationToken);
                return candidate;
            }
            catch (Exception ex)
            {
                lastFailure = ex;
                await _repository.RollbackTransactionAsync(cancellationToken);

                var concurrent = await _repository.GetOperationByIdempotencyKeyAsync(command.IdempotencyKey, cancellationToken);
                if (concurrent is not null)
                {
                    if (!SemanticallyMatches(concurrent, candidate))
                        throw new IdempotencyConflictException("The idempotency key is already associated with a different financial command.");

                    return concurrent;
                }

                if (ex is DomainValidationException or IdempotencyConflictException)
                    throw;

                var latestEffects = await _repository.GetRecoverableEffectsAsync(command.RecoverableId, cancellationToken);
                if (latestEffects.Count > 0 && amount.MinorUnits > latestEffects.Sum(x => x.SignedMinorUnits))
                    throw new DomainValidationException("Settlement amount cannot exceed the outstanding recoverable amount.");

                if (attempt < 3)
                    continue;
            }
        }

        throw lastFailure ?? new InvalidOperationException("Recoverable settlement failed.");
    }

    public async Task<FinancialOperation> AcceptReversalAsync(ReverseOperationCommand command, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(command.IdempotencyKey))
            throw new DomainValidationException("Idempotency key is required.");

        var existingByKey = await _repository.GetOperationByIdempotencyKeyAsync(command.IdempotencyKey, cancellationToken);
        if (existingByKey is not null)
        {
            if (existingByKey.Type != FinancialOperationType.Reversal ||
                existingByKey.OriginalOperationId != command.OriginalOperationId ||
                existingByKey.EffectiveAt != Parse(command.EffectiveAt))
                throw new IdempotencyConflictException("The idempotency key is already associated with a different financial command.");

            return existingByKey;
        }

        var original = await _repository.GetOperationAsync(command.OriginalOperationId, cancellationToken)
            ?? throw new DomainValidationException("Original operation was not found.");

        var existingReversal = await _repository.GetReversalByOriginalOperationIdAsync(command.OriginalOperationId, cancellationToken);
        if (existingReversal is not null)
            throw new DomainValidationException("The original operation has already been reversed.");

        var operation = FinancialOperation.Reversal(original, Parse(command.EffectiveAt)).Accept();

        foreach (var recoverableEffect in original.RecoverableEffects.Where(x => x.Direction == RecoverableEffectDirection.Increase))
        {
            var currentEffects = await _repository.GetRecoverableEffectsAsync(recoverableEffect.RecoverableId, cancellationToken);
            var outstanding = checked(currentEffects.Sum(x => x.SignedMinorUnits));
            if (outstanding < recoverableEffect.Amount.MinorUnits)
                throw new DomainValidationException("The recoverable operation cannot be reversed while its recoverable claim has been partially settled. Reverse the settlements first.");
        }

        await _repository.BeginTransactionAsync(cancellationToken);
        try
        {
            await _repository.AddAcceptedOperationAsync(operation, command.IdempotencyKey, cancellationToken);
            await _repository.SaveChangesAsync(cancellationToken);
            await _repository.CommitTransactionAsync(cancellationToken);
            return operation;
        }
        catch
        {
            await _repository.RollbackTransactionAsync(cancellationToken);

            var concurrent = await _repository.GetOperationByIdempotencyKeyAsync(command.IdempotencyKey, cancellationToken);
            if (concurrent is not null)
            {
                if (!SemanticallyMatches(concurrent, operation))
                    throw new IdempotencyConflictException("The idempotency key is already associated with a different financial command.");

                return concurrent;
            }

            var concurrentReversal = await _repository.GetReversalByOriginalOperationIdAsync(command.OriginalOperationId, cancellationToken);
            if (concurrentReversal is not null)
                throw new DomainValidationException("The original operation has already been reversed.");

            throw;
        }
    }

    public async Task<IReadOnlyList<FinancialEffect>> GetEffectsAsync(Guid accountId, CancellationToken cancellationToken) =>
        await _repository.GetEffectsAsync(accountId, cancellationToken);

    public async Task<IReadOnlyList<RecoverableEffect>> GetRecoverableEffectsAsync(Guid recoverableId, CancellationToken cancellationToken) =>
        await _repository.GetRecoverableEffectsAsync(recoverableId, cancellationToken);

    private async Task ValidateActiveAccountsAsync(FinancialOperation operation, CancellationToken cancellationToken)
    {
        foreach (var accountId in operation.Effects.Select(x => x.AccountId).Distinct())
        {
            var account = await _repository.GetAccountAsync(accountId, cancellationToken)
                ?? throw new DomainValidationException("Account was not found.");

            if (account.Status != AccountStatus.Active)
                throw new DomainValidationException("Financial operations are not allowed on a closed account.");
        }
    }

    private static DateTimeOffset Parse(string value) =>
        DateTimeOffset.Parse(value, System.Globalization.CultureInfo.InvariantCulture);

    private static bool SemanticallyMatches(FinancialOperation existing, FinancialOperation candidate)
    {
        if (existing.Type != candidate.Type ||
            existing.EffectiveAt != candidate.EffectiveAt ||
            existing.Effects.Count != candidate.Effects.Count ||
            existing.RecoverableEffects.Count != candidate.RecoverableEffects.Count ||
            existing.OriginalOperationId != candidate.OriginalOperationId)
            return false;

        var effectsMatch = existing.Effects
            .OrderBy(x => x.Order)
            .Zip(candidate.Effects.OrderBy(x => x.Order))
            .All(pair =>
                pair.First.AccountId == pair.Second.AccountId &&
                pair.First.Amount == pair.Second.Amount &&
                pair.First.Direction == pair.Second.Direction &&
                pair.First.Order == pair.Second.Order &&
                pair.First.EffectiveAt == pair.Second.EffectiveAt);

        if (!effectsMatch)
            return false;

        return existing.RecoverableEffects
            .OrderBy(x => x.Order)
            .Zip(candidate.RecoverableEffects.OrderBy(x => x.Order))
            .All(pair =>
                pair.First.RecoverableId == pair.Second.RecoverableId ||
                ((existing.Type == FinancialOperationType.RecoverableExpense && candidate.Type == FinancialOperationType.RecoverableExpense) ||
                 (existing.Type == FinancialOperationType.SharedExpense && candidate.Type == FinancialOperationType.SharedExpense)) &&
                 pair.First.Direction == pair.Second.Direction &&
                 pair.First.Amount == pair.Second.Amount &&
                 string.Equals(pair.First.CounterpartyName, pair.Second.CounterpartyName, StringComparison.Ordinal));
    }
}
