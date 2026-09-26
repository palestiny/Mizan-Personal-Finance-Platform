namespace Mizan.Application.Capture;

public sealed record CaptureProviderDiagnostics(
    decimal? Confidence,
    IReadOnlyList<string> Warnings)
{
    public static CaptureProviderDiagnostics Empty { get; } =
        new(null, Array.Empty<string>());
}

public sealed record CaptureProviderResult(
    bool Succeeded,
    CaptureInterpretation? Interpretation,
    CaptureProviderDiagnostics Diagnostics,
    string? FailureReason)
{
    public static CaptureProviderResult Success(
        CaptureInterpretation interpretation,
        CaptureProviderDiagnostics? diagnostics = null) =>
        new(true, interpretation, diagnostics ?? CaptureProviderDiagnostics.Empty, null);

    public static CaptureProviderResult Failure(string reason) =>
        new(false, null, CaptureProviderDiagnostics.Empty, reason);
}

public interface ICaptureProviderAdapter
{
    Task<CaptureProviderResult> InterpretAsync(
        CaptureInput input,
        CancellationToken cancellationToken = default);
}

public static class CaptureProviderResultValidator
{
    public static bool TryValidate(
        CaptureProviderResult result,
        out CaptureProviderResult validated,
        out string error)
    {
        if (!result.Succeeded)
        {
            validated = result;
            error = string.Empty;
            return false;
        }

        if (result.Interpretation is null)
        {
            validated = CaptureProviderResult.Failure("Provider returned no interpretation.");
            error = "Provider returned no interpretation.";
            return false;
        }

        if (!Enum.IsDefined(result.Interpretation.OperationType))
        {
            validated = CaptureProviderResult.Failure("Provider returned an unsupported operation type.");
            error = "Provider returned an unsupported operation type.";
            return false;
        }

        if (result.Interpretation.AmountMinorUnits is <= 0)
        {
            validated = CaptureProviderResult.Failure("Provider returned an invalid amount.");
            error = "Provider returned an invalid amount.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(result.Interpretation.Currency))
        {
            validated = CaptureProviderResult.Failure("Provider returned no currency.");
            error = "Provider returned no currency.";
            return false;
        }

        if (result.Diagnostics.Confidence is < 0 or > 1)
        {
            validated = CaptureProviderResult.Failure("Provider returned an invalid confidence value.");
            error = "Provider returned an invalid confidence value.";
            return false;
        }

        validated = result with
        {
            Interpretation = Normalize(result.Interpretation),
            Diagnostics = Normalize(result.Diagnostics)
        };
        error = string.Empty;
        return true;
    }

    private static CaptureInterpretation Normalize(CaptureInterpretation interpretation) =>
        interpretation with
        {
            Currency = interpretation.Currency!.Trim().ToUpperInvariant(),
            AccountReference = NormalizeReference(interpretation.AccountReference),
            DestinationAccountReference = NormalizeReference(interpretation.DestinationAccountReference),
            MissingFields = NormalizeList(interpretation.MissingFields),
            Ambiguities = NormalizeList(interpretation.Ambiguities)
        };

    private static CaptureProviderDiagnostics Normalize(CaptureProviderDiagnostics diagnostics) =>
        diagnostics with
        {
            Warnings = NormalizeList(diagnostics.Warnings) ?? Array.Empty<string>()
        };

    private static string? NormalizeReference(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static IReadOnlyList<string>? NormalizeList(IReadOnlyList<string>? values)
    {
        if (values is null || values.Count == 0)
            return null;

        return values
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Select(value => value.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }
}
