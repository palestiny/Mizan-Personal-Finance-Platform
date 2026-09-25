namespace Mizan.Application.Capture;

public enum CaptureOperationType { Income, PersonalExpense, OwnedAccountTransfer }

public sealed record CaptureInput(
    string RawText,
    string Channel,
    string? Locale,
    string? TimeZone);

public sealed record CaptureInterpretation(
    CaptureOperationType OperationType,
    long? AmountMinorUnits,
    string? Currency,
    DateTimeOffset? EffectiveAt,
    string? AccountReference,
    string? DestinationAccountReference,
    IReadOnlyList<string>? MissingFields,
    IReadOnlyList<string>? Ambiguities)
{
    public CaptureInterpretation(
        CaptureOperationType operationType,
        long? amountMinorUnits,
        string? currency,
        DateTimeOffset? effectiveAt,
        string? accountReference,
        IReadOnlyList<string>? missingFields,
        IReadOnlyList<string>? ambiguities)
        : this(operationType, amountMinorUnits, currency, effectiveAt, accountReference, null, missingFields, ambiguities)
    {
    }

    public Guid? AccountId { get; init; }
}

public interface ICaptureInterpreter
{
    CaptureInterpretation Interpret(CaptureInput input);
}

public sealed record CaptureResolution(
    Guid? AccountId,
    Guid? DestinationAccountId,
    IReadOnlyList<string> MissingFields,
    IReadOnlyList<string> Ambiguities)
{
    public bool IsComplete => MissingFields.Count == 0 && Ambiguities.Count == 0;
}

public interface ICaptureContextResolver
{
    CaptureResolution Resolve(CaptureInterpretation interpretation);
}
