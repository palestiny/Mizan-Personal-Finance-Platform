using System.Text.Json;

namespace Mizan.Application.Capture;

public sealed class CaptureProposalService(
    ICaptureInterpreter interpreter,
    ICaptureContextResolver resolver)
{
    public Proposal CreateProposal(CaptureInput input, DateTimeOffset expiresAt)
    {
        var interpretation = interpreter.Interpret(input);
        var resolution = resolver.Resolve(interpretation);

        var missing = resolution.MissingFields.Count == 0 ? null : string.Join(", ", resolution.MissingFields.Distinct(StringComparer.OrdinalIgnoreCase));
        var ambiguities = resolution.Ambiguities.Count == 0 ? null : string.Join(", ", resolution.Ambiguities.Distinct(StringComparer.OrdinalIgnoreCase));

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
            JsonSerializer.Serialize(new
            {
                Channel = input.Channel,
                Locale = input.Locale,
                TimeZone = input.TimeZone,
                AccountReference = interpretation.AccountReference,
                DestinationAccountReference = interpretation.DestinationAccountReference
            }),
            expiresAt);

        return proposal;
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
