using Mizan.Domain.Finance;

namespace Mizan.Application.Capture;

public enum ProposalStatus { Draft, ReadyForConfirmation, Confirmed, Rejected, Expired, Failed }
public enum ProposalOperationType { Income, PersonalExpense, OwnedAccountTransfer }

public sealed class Proposal
{
    private Proposal(Guid id,string originalInput,ProposalOperationType operationType,Guid? accountId,Guid? destinationAccountId,long? amountMinorUnits,string? currency,DateTimeOffset? effectiveAt,string? missingFields,string? ambiguities,string? interpretationMetadata,ProposalStatus status,DateTimeOffset createdAt,DateTimeOffset expiresAt,DateTimeOffset? confirmedAt,DateTimeOffset? rejectedAt,string? commandIdempotencyKey,Guid? operationId)
    { Id=id; OriginalInput=originalInput; OperationType=operationType; AccountId=accountId; DestinationAccountId=destinationAccountId; AmountMinorUnits=amountMinorUnits; Currency=currency; EffectiveAt=effectiveAt; MissingFields=missingFields; Ambiguities=ambiguities; InterpretationMetadata=interpretationMetadata; Status=status; CreatedAt=createdAt; ExpiresAt=expiresAt; ConfirmedAt=confirmedAt; RejectedAt=rejectedAt; CommandIdempotencyKey=commandIdempotencyKey; OperationId=operationId; }
    public Guid Id { get; } public string OriginalInput { get; } public ProposalOperationType OperationType { get; } public Guid? AccountId { get; } public Guid? DestinationAccountId { get; } public long? AmountMinorUnits { get; } public string? Currency { get; } public DateTimeOffset? EffectiveAt { get; } public string? MissingFields { get; } public string? Ambiguities { get; } public string? InterpretationMetadata { get; } public ProposalStatus Status { get; private set; } public DateTimeOffset CreatedAt { get; } public DateTimeOffset ExpiresAt { get; } public DateTimeOffset? ConfirmedAt { get; private set; } public DateTimeOffset? RejectedAt { get; private set; } public string? CommandIdempotencyKey { get; private set; } public Guid? OperationId { get; private set; }

    public static Proposal Create(string originalInput,ProposalOperationType operationType,Guid? accountId,Guid? destinationAccountId,long? amountMinorUnits,string? currency,DateTimeOffset? effectiveAt,string? missingFields,string? ambiguities,string? interpretationMetadata,DateTimeOffset expiresAt)
    {
        if(string.IsNullOrWhiteSpace(originalInput)) throw new DomainValidationException("Original input is required.");
        var expiry=expiresAt.ToUniversalTime(); if(expiry<=DateTimeOffset.UtcNow) throw new DomainValidationException("Proposal expiration must be in the future.");
        var missing=Normalize(missingFields); var ambiguous=Normalize(ambiguities);
        var status=missing is null && ambiguous is null ? ProposalStatus.ReadyForConfirmation : ProposalStatus.Draft;
        return new Proposal(Guid.NewGuid(),originalInput.Trim(),operationType,accountId,destinationAccountId,amountMinorUnits,Normalize(currency),effectiveAt?.ToUniversalTime(),missing,ambiguous,Normalize(interpretationMetadata),status,DateTimeOffset.UtcNow,expiry,null,null,null,null);
    }

    public static Proposal Rehydrate(Guid id,string originalInput,ProposalOperationType operationType,Guid? accountId,Guid? destinationAccountId,long? amountMinorUnits,string? currency,DateTimeOffset? effectiveAt,string? missingFields,string? ambiguities,string? interpretationMetadata,ProposalStatus status,DateTimeOffset createdAt,DateTimeOffset expiresAt,DateTimeOffset? confirmedAt,DateTimeOffset? rejectedAt,string? commandIdempotencyKey,Guid? operationId)
        => new(id,originalInput,operationType,accountId,destinationAccountId,amountMinorUnits,currency,effectiveAt,missingFields,ambiguities,interpretationMetadata,status,createdAt,expiresAt,confirmedAt,rejectedAt,commandIdempotencyKey,operationId);

    public void Prepare()
    {
        EnsureActive();
        if(Status!=ProposalStatus.Draft) throw new DomainValidationException("Only draft proposals can be prepared.");
        if(MissingFields is not null || Ambiguities is not null) throw new DomainValidationException("Proposal has unresolved required fields or ambiguities.");
        Status=ProposalStatus.ReadyForConfirmation;
    }
    public void Reject() { EnsureActive(); Status=ProposalStatus.Rejected; RejectedAt=DateTimeOffset.UtcNow; }
    public void Expire() { EnsureActive(); Status=ProposalStatus.Expired; }
    public void AssociateConfirmation(string key)
    {
        if(Status==ProposalStatus.ReadyForConfirmation && DateTimeOffset.UtcNow>=ExpiresAt)
        {
            Status=ProposalStatus.Expired;
            throw new DomainValidationException("Proposal has expired.");
        }
        if(Status!=ProposalStatus.ReadyForConfirmation && Status!=ProposalStatus.Confirmed) throw new DomainValidationException("Proposal is not confirmable.");
        if(string.IsNullOrWhiteSpace(key)) throw new DomainValidationException("Command idempotency key is required.");
        if(CommandIdempotencyKey is not null && !string.Equals(CommandIdempotencyKey,key,StringComparison.Ordinal)) throw new IdempotencyConflictException("The proposal is already associated with a different confirmation idempotency key.");
        CommandIdempotencyKey=key;
    }
    public void MarkConfirmed(Guid operationId) { if(Status!=ProposalStatus.ReadyForConfirmation && Status!=ProposalStatus.Confirmed) throw new DomainValidationException("Proposal is not confirmable."); Status=ProposalStatus.Confirmed; ConfirmedAt??=DateTimeOffset.UtcNow; OperationId=operationId; }
    public void MarkFailed() { if(Status!=ProposalStatus.Confirmed && Status!=ProposalStatus.ReadyForConfirmation) throw new DomainValidationException("Proposal cannot be marked failed from its current state."); Status=ProposalStatus.Failed; }
    private void EnsureActive() { if(DateTimeOffset.UtcNow>=ExpiresAt) { Status=ProposalStatus.Expired; throw new DomainValidationException("Proposal has expired."); } if(Status!=ProposalStatus.Draft && Status!=ProposalStatus.ReadyForConfirmation) throw new DomainValidationException("Proposal is not active."); }
    private static string? Normalize(string? value)=>string.IsNullOrWhiteSpace(value)?null:value.Trim();
}
