namespace Mizan.Application.Capture;
public sealed record CreateProposalCommand(string OriginalInput,ProposalOperationType OperationType,Guid? AccountId,Guid? DestinationAccountId,long? AmountMinorUnits,string? Currency,string? EffectiveAt,string? MissingFields,string? Ambiguities,string? InterpretationMetadata,string ExpiresAt);
public sealed record ConfirmProposalCommand(Guid ProposalId,string CommandIdempotencyKey);
public sealed record RejectProposalCommand(Guid ProposalId);
public sealed record ExpireProposalCommand(Guid ProposalId);
