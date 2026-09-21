using System.Data;
using Mizan.Domain.Finance;

namespace Mizan.Application.Finance;

public interface IFinanceRepository
{
    Task<Account> AddAccountAsync(Account account, CancellationToken cancellationToken);
    Task<Account?> GetAccountAsync(Guid accountId, CancellationToken cancellationToken);
    Task UpdateAccountAsync(Account account, CancellationToken cancellationToken);
    Task<FinancialOperation?> GetOperationAsync(Guid operationId, CancellationToken cancellationToken);
    Task<FinancialOperation?> GetOperationByIdempotencyKeyAsync(string idempotencyKey, CancellationToken cancellationToken);
    Task<FinancialOperation?> GetReversalByOriginalOperationIdAsync(Guid originalOperationId, CancellationToken cancellationToken);
    Task AddAcceptedOperationAsync(FinancialOperation operation, string idempotencyKey, CancellationToken cancellationToken);
    Task<IReadOnlyList<FinancialEffect>> GetEffectsAsync(Guid accountId, CancellationToken cancellationToken);
    Task<IReadOnlyList<RecoverableEffect>> GetRecoverableEffectsAsync(Guid recoverableId, CancellationToken cancellationToken);
    Task<Obligation?> GetObligationAsync(Guid obligationId, CancellationToken cancellationToken);
    Task<Obligation> AddObligationAsync(Obligation obligation, CancellationToken cancellationToken);
    Task UpdateObligationAsync(Obligation obligation, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
    Task BeginTransactionAsync(CancellationToken cancellationToken, IsolationLevel isolationLevel = IsolationLevel.ReadCommitted);
    Task CommitTransactionAsync(CancellationToken cancellationToken);
    Task RollbackTransactionAsync(CancellationToken cancellationToken);
}
