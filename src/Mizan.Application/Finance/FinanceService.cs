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
                if (concurrent.Type != operation.Type ||
                    concurrent.OriginalOperationId != operation.OriginalOperationId ||
                    concurrent.EffectiveAt != operation.EffectiveAt)
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
            existing.OriginalOperationId != candidate.OriginalOperationId)
            return false;

        return existing.Effects
            .OrderBy(x => x.Order)
            .Zip(candidate.Effects.OrderBy(x => x.Order))
            .All(pair =>
                pair.First.AccountId == pair.Second.AccountId &&
                pair.First.Amount == pair.Second.Amount &&
                pair.First.Direction == pair.Second.Direction &&
                pair.First.Order == pair.Second.Order &&
                pair.First.EffectiveAt == pair.Second.EffectiveAt);
    }
}
