namespace Mizan.Application.Capture;

public sealed record ProviderCaptureProposalResult(
    bool Succeeded,
    Proposal? Proposal,
    string? FailureReason)
{
    public static ProviderCaptureProposalResult Success(Proposal proposal) =>
        new(true, proposal, null);

    public static ProviderCaptureProposalResult Failure(string reason) =>
        new(false, null, reason);
}

public sealed class ProviderCaptureProposalService(
    ICaptureProviderAdapter provider,
    ICaptureContextResolver resolver)
{
    public async Task<ProviderCaptureProposalResult> CreateProposalAsync(
        CaptureInput input,
        DateTimeOffset expiresAt,
        CancellationToken cancellationToken = default)
    {
        var providerResult = await provider.InterpretAsync(input, cancellationToken);

        if (!CaptureProviderResultValidator.TryValidate(
                providerResult,
                out var validated,
                out var validationError))
        {
            return ProviderCaptureProposalResult.Failure(
                string.IsNullOrWhiteSpace(validationError)
                    ? validated.FailureReason ?? "Capture provider failed."
                    : validationError);
        }

        var interpretation = validated.Interpretation!;
        var resolution = resolver.Resolve(interpretation);

        var missing = resolution.MissingFields.Count == 0
            ? null
            : string.Join(", ", resolution.MissingFields.Distinct(StringComparer.OrdinalIgnoreCase));

        var ambiguities = resolution.Ambiguities.Count == 0
            ? null
            : string.Join(", ", resolution.Ambiguities.Distinct(StringComparer.OrdinalIgnoreCase));

        var proposal = Proposal.Create(
            input.RawText,
            MapOperation(interpretation.OperationType),
            resolution.AccountId,
            resolution.DestinationAccountId,
            interpretation.AmountMinorUnits,
            interpretation.Currency,
            interpretation.EffectiveAt,
            missing,
            ambiguities,
            null,
            expiresAt);

        return ProviderCaptureProposalResult.Success(proposal);
    }

    private static ProposalOperationType MapOperation(CaptureOperationType operation) =>
        operation switch
        {
            CaptureOperationType.Income => ProposalOperationType.Income,
            CaptureOperationType.PersonalExpense => ProposalOperationType.PersonalExpense,
            CaptureOperationType.OwnedAccountTransfer => ProposalOperationType.OwnedAccountTransfer,
            _ => throw new ArgumentOutOfRangeException(nameof(operation))
        };
}
