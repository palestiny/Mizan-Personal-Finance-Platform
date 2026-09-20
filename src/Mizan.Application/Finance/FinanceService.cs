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
            throw;
        }
    }

    public async Task<IReadOnlyList<FinancialEffect>> GetEffectsAsync(Guid accountId, CancellationToken cancellationToken) =>
        await _repository.GetEffectsAsync(accountId, cancellationToken);
}

    private static bool SemanticallyMatches(FinancialOperation existing, FinancialOperation candidate)
    {
        if (existing.Type != candidate.Type ||
            existing.EffectiveAt != candidate.EffectiveAt ||
            existing.Effects.Count != candidate.Effects.Count)
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