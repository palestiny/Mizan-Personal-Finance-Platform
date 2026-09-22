using System.Data;
using System.Globalization;
using Mizan.Application.Finance;
using Mizan.Domain.Finance;

namespace Mizan.Application.Capture;

public sealed class ProposalService(IProposalRepository proposals,FinanceService finance)
{
    public async Task<Proposal> CreateAsync(CreateProposalCommand command,CancellationToken ct)
    {
        var proposal=Proposal.Create(command.OriginalInput,command.OperationType,command.AccountId,command.DestinationAccountId,command.AmountMinorUnits,command.Currency,ParseNullable(command.EffectiveAt),command.MissingFields,command.Ambiguities,command.InterpretationMetadata,Parse(command.ExpiresAt));
        ValidateShape(proposal);
        await proposals.AddAsync(proposal,ct); await proposals.SaveChangesAsync(ct); return proposal;
    }
    public Task<Proposal?> GetAsync(Guid id,CancellationToken ct)=>proposals.GetAsync(id,ct);
    public async Task<Proposal> PrepareAsync(Guid id,CancellationToken ct)
    {
        var p=await GetRequired(id,ct); p.Prepare(); ValidateShape(p); await proposals.UpdateAsync(p,ct); await proposals.SaveChangesAsync(ct); return p;
    }
    public async Task<Proposal> RejectAsync(RejectProposalCommand c,CancellationToken ct){var p=await GetRequired(c.ProposalId,ct); p.Reject(); await proposals.UpdateAsync(p,ct); await proposals.SaveChangesAsync(ct); return p;}
    public async Task<Proposal> ExpireAsync(ExpireProposalCommand c,CancellationToken ct){var p=await GetRequired(c.ProposalId,ct); p.Expire(); await proposals.UpdateAsync(p,ct); await proposals.SaveChangesAsync(ct); return p;}

    public async Task<FinancialOperation> ConfirmAsync(ConfirmProposalCommand c,CancellationToken ct)
    {
        if(string.IsNullOrWhiteSpace(c.CommandIdempotencyKey)) throw new DomainValidationException("Command idempotency key is required.");
        var p=await ClaimAsync(c,ct);
        if(p.OperationId is Guid existingId){var existing=await finance.GetOperationAsync(existingId,ct); if(existing is not null)return existing;}
        if(p.CommandIdempotencyKey is null) throw new InvalidOperationException("Confirmed proposal has no command idempotency key.");
        try
        {
            var accepted=await finance.AcceptAsync(BuildOperation(p),p.CommandIdempotencyKey,ct);
            p.MarkConfirmed(accepted.Id); await proposals.UpdateAsync(p,ct); await proposals.SaveChangesAsync(ct); return accepted;
        }
        catch(DomainValidationException){await MarkFailedAsync(p.Id,ct); throw;}
        catch(IdempotencyConflictException){await MarkFailedAsync(p.Id,ct); throw;}
    }

    private async Task<Proposal> ClaimAsync(ConfirmProposalCommand c,CancellationToken ct)
    {
        Exception? last=null;
        for(var attempt=1;attempt<=3;attempt++)
        {
            await proposals.BeginTransactionAsync(ct,IsolationLevel.Serializable);
            try
            {
                var p=await GetRequired(c.ProposalId,ct); p.AssociateConfirmation(c.CommandIdempotencyKey);
                await proposals.UpdateAsync(p,ct); await proposals.SaveChangesAsync(ct); await proposals.CommitTransactionAsync(ct); return p;
            }
            catch(Exception ex)
            {
                last=ex; await proposals.RollbackTransactionAsync(ct);
                if(ex is DomainValidationException or IdempotencyConflictException)throw;
                if(attempt<3)continue;
            }
        }
        throw last??new InvalidOperationException("Proposal confirmation claim failed.");
    }

    private async Task MarkFailedAsync(Guid id,CancellationToken ct)
    {
        await proposals.BeginTransactionAsync(ct,IsolationLevel.Serializable);
        try{var p=await GetRequired(id,ct); if(p.Status==ProposalStatus.Confirmed||p.Status==ProposalStatus.ReadyForConfirmation){p.MarkFailed();await proposals.UpdateAsync(p,ct);await proposals.SaveChangesAsync(ct);}await proposals.CommitTransactionAsync(ct);}
        catch{await proposals.RollbackTransactionAsync(ct);throw;}
    }

    private static FinancialOperation.OperationBuilder BuildOperation(Proposal p)
    {
        var money=RequiredMoney(p); var effective=Required(p.EffectiveAt,"EffectiveAt"); var account=Required(p.AccountId,"AccountId");
        return p.OperationType switch
        {
            ProposalOperationType.Income=>FinancialOperation.Income(account,money,effective),
            ProposalOperationType.PersonalExpense=>FinancialOperation.PersonalExpense(account,money,effective),
            ProposalOperationType.OwnedAccountTransfer=>FinancialOperation.OwnedAccountTransfer(account,Required(p.DestinationAccountId,"DestinationAccountId"),money,effective),
            _=>throw new DomainValidationException("Unsupported proposal operation type.")
        };
    }
    private static void ValidateShape(Proposal p)
    {
        if(p.Status==ProposalStatus.Draft)return;
        _=RequiredMoney(p); _=Required(p.AccountId,"AccountId"); _=Required(p.EffectiveAt,"EffectiveAt");
        if(p.OperationType==ProposalOperationType.OwnedAccountTransfer)_=Required(p.DestinationAccountId,"DestinationAccountId");
    }
    private static Money RequiredMoney(Proposal p)=>p.AmountMinorUnits is long amount&&p.Currency is not null?Money.FromMinorUnits(amount,p.Currency):throw new DomainValidationException("Amount and currency are required.");
    private static T Required<T>(T? value,string name) where T:struct=>value??throw new DomainValidationException($"{name} is required.");
    private static DateTimeOffset? ParseNullable(string? value)=>string.IsNullOrWhiteSpace(value)?null:Parse(value);
    private static DateTimeOffset Parse(string value)=>DateTimeOffset.Parse(value,CultureInfo.InvariantCulture,DateTimeStyles.RoundtripKind);
    private async Task<Proposal> GetRequired(Guid id,CancellationToken ct)=>await proposals.GetAsync(id,ct)??throw new DomainValidationException("Proposal was not found.");
}
