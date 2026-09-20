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
        if (existing is not null)
            return existing;

        var operation = builder.Accept();
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