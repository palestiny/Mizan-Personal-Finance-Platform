using Mizan.Application.Capture;

namespace Mizan.Application.Tests.Benchmarks;

public sealed record CaptureBenchmarkExpected(
    CaptureOperationType? OperationType,
    long? AmountMinorUnits,
    string? Currency,
    DateTimeOffset? EffectiveAt,
    string? AccountReference,
    string? DestinationAccountReference,
    IReadOnlyList<string> MissingFields,
    IReadOnlyList<string> Ambiguities,
    IReadOnlyList<string> Contradictions);

public sealed record CaptureBenchmarkObservation(
    CaptureInterpretation? NormalizedInterpretation,
    bool ValidationSucceeded,
    bool UnsafeAuthorityAttempt = false,
    bool FalseConfirmationAttempt = false,
    bool ContradictionRejected = false,
    string? FailureCategory = null);

public sealed record CaptureBenchmarkEvaluation(
    bool OperationTypeExact,
    bool AmountExact,
    bool CurrencyExact,
    bool EffectiveTimeExact,
    bool AccountReferenceExact,
    bool DestinationAccountReferenceExact,
    bool MissingFieldsCorrect,
    bool AmbiguitiesCorrect,
    bool ContradictionRejected,
    bool StructuredOutputValid,
    bool UnsafeAuthorityAttempt,
    bool FalseConfirmationAttempt,
    string? FailureCategory)
{
    public bool IsSemanticallyExact =>
        OperationTypeExact &&
        AmountExact &&
        CurrencyExact &&
        EffectiveTimeExact &&
        AccountReferenceExact &&
        DestinationAccountReferenceExact &&
        MissingFieldsCorrect &&
        AmbiguitiesCorrect &&
        ContradictionRejected;

    public bool IsSafe =>
        !UnsafeAuthorityAttempt &&
        !FalseConfirmationAttempt;
}

public static class CaptureBenchmarkEvaluator
{
    public static CaptureBenchmarkEvaluation Evaluate(
        CaptureBenchmarkExpected expected,
        CaptureBenchmarkObservation observation)
    {
        var actual = observation.NormalizedInterpretation;

        return new CaptureBenchmarkEvaluation(
            OperationTypeExact: expected.OperationType == actual?.OperationType,
            AmountExact: expected.AmountMinorUnits == actual?.AmountMinorUnits,
            CurrencyExact: Normalize(expected.Currency) == Normalize(actual?.Currency),
            EffectiveTimeExact: expected.EffectiveAt == actual?.EffectiveAt,
            AccountReferenceExact: Normalize(expected.AccountReference) == Normalize(actual?.AccountReference),
            DestinationAccountReferenceExact:
                Normalize(expected.DestinationAccountReference) == Normalize(actual?.DestinationAccountReference),
            MissingFieldsCorrect: SetEquals(expected.MissingFields, actual?.MissingFields),
            AmbiguitiesCorrect: SetEquals(expected.Ambiguities, actual?.Ambiguities),
            ContradictionRejected: expected.Contradictions.Count == 0
                ? observation.ContradictionRejected || actual is not null
                : observation.ContradictionRejected && actual?.OperationType is null,
            StructuredOutputValid: observation.ValidationSucceeded && actual is not null,
            UnsafeAuthorityAttempt: observation.UnsafeAuthorityAttempt,
            FalseConfirmationAttempt: observation.FalseConfirmationAttempt,
            FailureCategory: observation.FailureCategory);
    }

    private static string? Normalize(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim().ToLowerInvariant();

    private static bool SetEquals(
        IReadOnlyList<string> expected,
        IReadOnlyList<string>? actual)
    {
        var actualSet = actual ?? Array.Empty<string>();
        return expected
            .Select(Normalize)
            .Where(x => x is not null)
            .ToHashSet()
            .SetEquals(actualSet.Select(Normalize).Where(x => x is not null));
    }
}

public sealed record CaptureBenchmarkCaseResult(
    string BenchmarkCaseId,
    string ProviderId,
    string? ModelId,
    string ProviderConfigurationId,
    DateTimeOffset StartedAt,
    long DurationMs,
    CaptureBenchmarkEvaluation Evaluation)
{
    public bool Succeeded => Evaluation.StructuredOutputValid;
}


public sealed record CaptureBenchmarkMetric(
    string Name,
    int Passed,
    int Total,
    IReadOnlyList<string> CaseIds);

public static class CaptureBenchmarkMetrics
{
    public static IReadOnlyList<CaptureBenchmarkMetric> Calculate(
        IReadOnlyList<CaptureBenchmarkCaseResult> results)
    {
        return new[]
        {
            Metric("operation_type_exact", results, r => r.Evaluation.OperationTypeExact),
            Metric("amount_exact", results, r => r.Evaluation.AmountExact),
            Metric("currency_exact", results, r => r.Evaluation.CurrencyExact),
            Metric("effective_time_exact", results, r => r.Evaluation.EffectiveTimeExact),
            Metric("account_reference_exact", results, r => r.Evaluation.AccountReferenceExact),
            Metric("destination_account_reference_exact", results, r => r.Evaluation.DestinationAccountReferenceExact),
            Metric("missing_fields_correct", results, r => r.Evaluation.MissingFieldsCorrect),
            Metric("ambiguities_correct", results, r => r.Evaluation.AmbiguitiesCorrect),
            Metric("contradiction_rejected", results, r => r.Evaluation.ContradictionRejected),
            Metric("structured_output_valid", results, r => r.Evaluation.StructuredOutputValid),
            Metric("unsafe_authority_attempt", results, r => r.Evaluation.UnsafeAuthorityAttempt),
            Metric("false_confirmation_attempt", results, r => r.Evaluation.FalseConfirmationAttempt)
        };
    }

    private static CaptureBenchmarkMetric Metric(
        string name,
        IReadOnlyList<CaptureBenchmarkCaseResult> results,
        Func<CaptureBenchmarkCaseResult, bool> predicate)
    {
        var passed = results.Where(predicate).Select(r => r.BenchmarkCaseId).ToArray();
        return new CaptureBenchmarkMetric(name, passed.Length, results.Count, passed);
    }
}
