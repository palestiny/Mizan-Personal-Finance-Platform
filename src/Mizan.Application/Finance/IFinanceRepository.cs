using Mizan.Domain.Finance;

namespace Mizan.Application.Finance;

public interface IFinanceRepository
{
    Task<Account> AddAccountAsync(Account account, CancellationToken cancellationToken);
    Task<Account?> GetAccountAsync(Guid accountId, CancellationToken cancellationToken);
    Task<FinancialOperation?> GetOperationByIdempotencyKeyAsync(string idempotencyKey, CancellationToken cancellationToken);
    Task AddAcceptedOperationAsync(FinancialOperation operation, string idempotencyKey, CancellationToken cancellationToken);
    Task<IReadOnlyList<FinancialEffect>> GetEffectsAsync(Guid accountId, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
    Task BeginTransactionAsync(CancellationToken cancellationToken);
    Task CommitTransactionAsync(CancellationToken cancellationToken);
    Task RollbackTransactionAsync(CancellationToken cancellationToken);
}