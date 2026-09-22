using System.Data;
using Microsoft.EntityFrameworkCore;
using Mizan.Application.Capture;

namespace Mizan.Infrastructure.Persistence;

public sealed class EfProposalRepository(MizanDbContext db) : IProposalRepository
{
    private Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction? _transaction;

    public async Task<Proposal?> GetAsync(Guid proposalId, CancellationToken ct)
    {
        var r=await db.Proposals.AsNoTracking().SingleOrDefaultAsync(x=>x.Id==proposalId,ct);
        return r is null?null:ToDomain(r);
    }

    public Task<Proposal> AddAsync(Proposal proposal, CancellationToken ct)
    {
        db.Proposals.Add(ToRecord(proposal));
        return Task.FromResult(proposal);
    }

    public async Task UpdateAsync(Proposal proposal, CancellationToken ct)
    {
        var r=await db.Proposals.SingleAsync(x=>x.Id==proposal.Id,ct);
        r.Status=(int)proposal.Status; r.ConfirmedAt=proposal.ConfirmedAt; r.RejectedAt=proposal.RejectedAt;
        r.CommandIdempotencyKey=proposal.CommandIdempotencyKey; r.OperationId=proposal.OperationId;
    }

    public Task SaveChangesAsync(CancellationToken ct)=>db.SaveChangesAsync(ct);
    public async Task BeginTransactionAsync(CancellationToken ct, IsolationLevel isolationLevel=IsolationLevel.ReadCommitted)=>_transaction=await db.Database.BeginTransactionAsync(isolationLevel,ct);
    public async Task CommitTransactionAsync(CancellationToken ct){if(_transaction is not null)await _transaction.CommitAsync(ct);}
    public async Task RollbackTransactionAsync(CancellationToken ct){if(_transaction is not null)await _transaction.RollbackAsync(ct);}

    private static ProposalRecord ToRecord(Proposal p)=>new()
    {
        Id=p.Id, OriginalInput=p.OriginalInput, OperationType=(int)p.OperationType, AccountId=p.AccountId,
        DestinationAccountId=p.DestinationAccountId, AmountMinorUnits=p.AmountMinorUnits, Currency=p.Currency,
        EffectiveAt=p.EffectiveAt, MissingFields=p.MissingFields, Ambiguities=p.Ambiguities,
        InterpretationMetadata=p.InterpretationMetadata, Status=(int)p.Status, CreatedAt=p.CreatedAt,
        ExpiresAt=p.ExpiresAt, ConfirmedAt=p.ConfirmedAt, RejectedAt=p.RejectedAt,
        CommandIdempotencyKey=p.CommandIdempotencyKey, OperationId=p.OperationId
    };

    private static Proposal ToDomain(ProposalRecord r)=>Proposal.Rehydrate(
        r.Id,r.OriginalInput,(ProposalOperationType)r.OperationType,r.AccountId,r.DestinationAccountId,
        r.AmountMinorUnits,r.Currency,r.EffectiveAt,r.MissingFields,r.Ambiguities,r.InterpretationMetadata,
        (ProposalStatus)r.Status,r.CreatedAt,r.ExpiresAt,r.ConfirmedAt,r.RejectedAt,r.CommandIdempotencyKey,r.OperationId);
}
