using System.Data;
namespace Mizan.Application.Capture;
public interface IProposalRepository
{
    Task<Proposal?> GetAsync(Guid proposalId,CancellationToken cancellationToken);
    Task<Proposal> AddAsync(Proposal proposal,CancellationToken cancellationToken);
    Task UpdateAsync(Proposal proposal,CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
    Task BeginTransactionAsync(CancellationToken cancellationToken,IsolationLevel isolationLevel=IsolationLevel.ReadCommitted);
    Task CommitTransactionAsync(CancellationToken cancellationToken);
    Task RollbackTransactionAsync(CancellationToken cancellationToken);
}
