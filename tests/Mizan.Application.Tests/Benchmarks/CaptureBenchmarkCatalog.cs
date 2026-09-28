using System.Text.Json;
using Mizan.Application.Capture;

namespace Mizan.Application.Tests.Benchmarks;

public sealed record CaptureBenchmarkCase(
    string Id,
    string Channel,
    string? Locale,
    string Input,
    CaptureBenchmarkExpected Expected,
    IReadOnlyList<string> RequiredFields,
    IReadOnlyList<string> MissingFields,
    IReadOnlyList<string> Ambiguities,
    IReadOnlyList<string> Contradictions,
    IReadOnlyList<string> AdversarialConstraints);

public static class CaptureBenchmarkCatalog
{
    public static IReadOnlyList<CaptureBenchmarkCase> Load(string json)
    {
        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;
        var schemaVersion = root.GetProperty("schema_version").GetString();

        if (!string.Equals(schemaVersion, "m3-capture-benchmark-v1", StringComparison.Ordinal))
            throw new InvalidOperationException($"Unsupported benchmark schema: {schemaVersion}");

        return root.GetProperty("cases").EnumerateArray().Select(ParseCase).ToArray();
    }

    private static CaptureBenchmarkCase ParseCase(JsonElement element)
    {
        var expected = element.GetProperty("expected");

        return new CaptureBenchmarkCase(
            element.GetProperty("id").GetString() ?? throw new InvalidOperationException("Case id is required."),
            element.GetProperty("channel").GetString() ?? throw new InvalidOperationException("Channel is required."),
            element.TryGetProperty("locale", out var locale) ? locale.GetString() : null,
            element.GetProperty("input").GetString() ?? throw new InvalidOperationException("Input is required."),
            new CaptureBenchmarkExpected(
                ParseOperationType(expected),
                GetNullableInt64(expected, "amount_minor_units"),
                GetNullableString(expected, "currency"),
                GetNullableDateTimeOffset(expected, "effective_at"),
                GetNullableString(expected, "account_reference"),
                GetNullableString(expected, "destination_account_reference"),
                ReadStrings(element, "missing_fields"),
                ReadStrings(element, "ambiguities"),
                ReadStrings(element, "contradictions")),
            ReadStrings(element, "required_fields"),
            ReadStrings(element, "missing_fields"),
            ReadStrings(element, "ambiguities"),
            ReadStrings(element, "contradictions"),
            ReadStrings(element, "adversarial_constraints"));
    }

    private static CaptureOperationType? ParseOperationType(JsonElement expected)
    {
        var value = GetNullableString(expected, "operation_type");
        return value is null ? null :
            Enum.TryParse<CaptureOperationType>(value, out var result)
                ? result
                : throw new InvalidOperationException($"Unsupported operation type: {value}");
    }

    private static IReadOnlyList<string> ReadStrings(JsonElement parent, string property) =>
        parent.GetProperty(property).EnumerateArray()
            .Select(x => x.GetString() ?? throw new InvalidOperationException($"{property} contains null."))
            .ToArray();

    private static string? GetNullableString(JsonElement parent, string property) =>
        parent.GetProperty(property).ValueKind == JsonValueKind.Null ? null : parent.GetProperty(property).GetString();

    private static long? GetNullableInt64(JsonElement parent, string property) =>
        parent.GetProperty(property).ValueKind == JsonValueKind.Null ? null : parent.GetProperty(property).GetInt64();

    private static DateTimeOffset? GetNullableDateTimeOffset(JsonElement parent, string property) =>
        parent.GetProperty(property).ValueKind == JsonValueKind.Null ? null : parent.GetProperty(property).GetDateTimeOffset();
}

public sealed record CaptureBenchmarkProviderObservation(
    CaptureInterpretation? NormalizedInterpretation,
    bool ValidationSucceeded,
    bool UnsafeAuthorityAttempt = false,
    bool FalseConfirmationAttempt = false,
    bool ContradictionRejected = false,
    string? FailureCategory = null);

public interface ICaptureBenchmarkProvider
{
    ValueTask<CaptureBenchmarkProviderObservation> ExecuteAsync(
        CaptureBenchmarkCase benchmarkCase,
        CancellationToken cancellationToken = default);
}

public sealed class CaptureBenchmarkRunner
{
    private readonly ICaptureBenchmarkProvider _provider;

    public CaptureBenchmarkRunner(ICaptureBenchmarkProvider provider) => _provider = provider;

    public async Task<IReadOnlyList<CaptureBenchmarkCaseResult>> RunAsync(
        IReadOnlyList<CaptureBenchmarkCase> cases,
        string providerId,
        string? modelId,
        string providerConfigurationId,
        CancellationToken cancellationToken = default)
    {
        var results = new List<CaptureBenchmarkCaseResult>(cases.Count);

        foreach (var benchmarkCase in cases)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var startedAt = DateTimeOffset.UtcNow;
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            var observation = await _provider.ExecuteAsync(benchmarkCase, cancellationToken);
            stopwatch.Stop();

            var evaluation = CaptureBenchmarkEvaluator.Evaluate(
                benchmarkCase.Expected,
                new CaptureBenchmarkObservation(
                    observation.NormalizedInterpretation,
                    observation.ValidationSucceeded,
                    observation.UnsafeAuthorityAttempt,
                    observation.FalseConfirmationAttempt,
                    observation.ContradictionRejected,
                    observation.FailureCategory));

            results.Add(new CaptureBenchmarkCaseResult(
                benchmarkCase.Id, providerId, modelId, providerConfigurationId,
                startedAt, stopwatch.ElapsedMilliseconds, evaluation));
        }

        return results;
    }
}
