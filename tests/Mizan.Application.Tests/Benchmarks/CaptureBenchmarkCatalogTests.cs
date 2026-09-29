using FluentAssertions;
using Mizan.Application.Capture;
using Xunit;

namespace Mizan.Application.Tests.Benchmarks;

public sealed class CaptureBenchmarkCatalogTests
{
    [Fact]
    public void Catalog_loads_seed_cases_without_provider_specific_semantics()
    {
        const string json = """
        {
          "schema_version": "m3-capture-benchmark-v1",
          "cases": [{
            "id": "TXT-AR-001",
            "channel": "text",
            "locale": "ar-EG",
            "input": "دفعت 250 جنيه من الكاش",
            "expected": {
              "operation_type": "PersonalExpense",
              "amount_minor_units": 25000,
              "currency": "EGP",
              "effective_at": null,
              "account_reference": "cash",
              "destination_account_reference": null
            },
            "required_fields": ["operation_type", "amount_minor_units", "currency", "account_reference"],
            "missing_fields": [],
            "ambiguities": [],
            "contradictions": [],
            "adversarial_constraints": []
          }]
        }
        """;

        var cases = CaptureBenchmarkCatalog.Load(json);

        cases.Should().ContainSingle();
        cases[0].Expected.OperationType.Should().Be(CaptureOperationType.PersonalExpense);
        cases[0].Expected.AccountReference.Should().Be("cash");
        cases[0].AdversarialConstraints.Should().BeEmpty();
    }

    [Fact]
    public void Catalog_rejects_unknown_schema_versions()
    {
        const string json = """{"schema_version":"future-version","cases":[]}""";

        var action = () => CaptureBenchmarkCatalog.Load(json);

        action.Should().Throw<InvalidOperationException>().WithMessage("*future-version*");
    }
    [Fact]
    public void Catalog_rejects_duplicate_case_ids()
    {
        const string json = """
        {
          "schema_version": "m3-capture-benchmark-v1",
          "cases": [
            {
              "id": "DUP-001",
              "channel": "text",
              "locale": "en",
              "input": "I spent 10 EGP from cash",
              "expected": {
                "operation_type": "PersonalExpense",
                "amount_minor_units": 1000,
                "currency": "EGP",
                "effective_at": null,
                "account_reference": "cash",
                "destination_account_reference": null
              },
              "required_fields": [],
              "missing_fields": [],
              "ambiguities": [],
              "contradictions": [],
              "adversarial_constraints": []
            },
            {
              "id": "DUP-001",
              "channel": "text",
              "locale": "en",
              "input": "I spent 20 EGP from cash",
              "expected": {
                "operation_type": "PersonalExpense",
                "amount_minor_units": 2000,
                "currency": "EGP",
                "effective_at": null,
                "account_reference": "cash",
                "destination_account_reference": null
              },
              "required_fields": [],
              "missing_fields": [],
              "ambiguities": [],
              "contradictions": [],
              "adversarial_constraints": []
            }
          ]
        }
        """;

        var action = () => CaptureBenchmarkCatalog.Load(json);

        action.Should().Throw<InvalidOperationException>().WithMessage("*DUP-001*");
    }

    [Fact]
    public void Catalog_rejects_unknown_channels()
    {
        const string json = """
        {
          "schema_version": "m3-capture-benchmark-v1",
          "cases": [{
            "id": "BAD-CHANNEL-001",
            "channel": "email",
            "locale": "en",
            "input": "I spent 10 EGP from cash",
            "expected": {
              "operation_type": "PersonalExpense",
              "amount_minor_units": 1000,
              "currency": "EGP",
              "effective_at": null,
              "account_reference": "cash",
              "destination_account_reference": null
            },
            "required_fields": [],
            "missing_fields": [],
            "ambiguities": [],
            "contradictions": [],
            "adversarial_constraints": []
          }]
        }
        """;

        var action = () => CaptureBenchmarkCatalog.Load(json);

        action.Should().Throw<InvalidOperationException>().WithMessage("*email*");
    }

}

public sealed class CaptureBenchmarkRunnerTests
{
    [Fact]
    public async Task Runner_evaluates_each_case_and_preserves_case_identity()
    {
        var cases = new[]
        {
            new CaptureBenchmarkCase(
                "TXT-001", "text", "en", "I spent 100 EGP from cash",
                new CaptureBenchmarkExpected(
                    CaptureOperationType.PersonalExpense, 10000, "EGP", null, "cash", null,
                    Array.Empty<string>(), Array.Empty<string>(), Array.Empty<string>()),
                new[] { "operation_type", "amount_minor_units", "currency", "account_reference" },
                Array.Empty<string>(), Array.Empty<string>(), Array.Empty<string>(), Array.Empty<string>())
        };

        var runner = new CaptureBenchmarkRunner(new FakeBenchmarkProvider());
        var results = await runner.RunAsync(cases, "fake-provider", "fake-model", "cfg-test");

        results.Should().ContainSingle();
        results[0].BenchmarkCaseId.Should().Be("TXT-001");
        results[0].ProviderId.Should().Be("fake-provider");
        results[0].Evaluation.IsSemanticallyExact.Should().BeTrue();
    }

    private sealed class FakeBenchmarkProvider : ICaptureBenchmarkProvider
    {
        public ValueTask<CaptureBenchmarkProviderObservation> ExecuteAsync(
            CaptureBenchmarkCase benchmarkCase,
            CancellationToken cancellationToken = default) =>
            ValueTask.FromResult(new CaptureBenchmarkProviderObservation(
                new CaptureInterpretation(
                    CaptureOperationType.PersonalExpense, 10000, "EGP", null, "cash", null,
                    Array.Empty<string>(), Array.Empty<string>()),
                ValidationSucceeded: true));
    }
}
