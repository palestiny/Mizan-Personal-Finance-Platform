using FluentAssertions;
using Mizan.Application.Capture;
using Xunit;

namespace Mizan.Application.Tests.Benchmarks;

public sealed class CaptureBenchmarkEvaluatorTests
{
    [Fact]
    public void Exact_complete_interpretation_passes_semantic_checks()
    {
        var expected = new CaptureBenchmarkExpected(
            CaptureOperationType.PersonalExpense, 25000, "EGP", null, "cash", null,
            Array.Empty<string>(), Array.Empty<string>(), Array.Empty<string>());

        var actual = new CaptureInterpretation(
            CaptureOperationType.PersonalExpense, 25000, "EGP", null, "cash", null,
            Array.Empty<string>(), Array.Empty<string>());

        var result = CaptureBenchmarkEvaluator.Evaluate(
            expected,
            new CaptureBenchmarkObservation(actual, ValidationSucceeded: true));

        result.IsSemanticallyExact.Should().BeTrue();
        result.IsSafe.Should().BeTrue();
        result.StructuredOutputValid.Should().BeTrue();
    }

    [Fact]
    public void Missing_account_must_be_preserved()
    {
        var expected = new CaptureBenchmarkExpected(
            CaptureOperationType.PersonalExpense, 30000, "EGP", null, null, null,
            new[] { "account_reference" }, Array.Empty<string>(), Array.Empty<string>());

        var actual = new CaptureInterpretation(
            CaptureOperationType.PersonalExpense, 30000, "EGP", null, null, null,
            new[] { "account_reference" }, Array.Empty<string>());

        var result = CaptureBenchmarkEvaluator.Evaluate(
            expected,
            new CaptureBenchmarkObservation(actual, ValidationSucceeded: true));

        result.MissingFieldsCorrect.Should().BeTrue();
        result.IsSemanticallyExact.Should().BeTrue();
    }

    [Fact]
    public void Ambiguous_account_must_not_be_resolved_by_the_provider()
    {
        var expected = new CaptureBenchmarkExpected(
            CaptureOperationType.PersonalExpense, 40000, "EGP", null, null, null,
            Array.Empty<string>(), new[] { "account_reference" }, Array.Empty<string>());

        var actual = new CaptureInterpretation(
            CaptureOperationType.PersonalExpense, 40000, "EGP", null, null, null,
            Array.Empty<string>(), new[] { "account_reference" });

        var result = CaptureBenchmarkEvaluator.Evaluate(
            expected,
            new CaptureBenchmarkObservation(actual, ValidationSucceeded: true));

        result.AmbiguitiesCorrect.Should().BeTrue();
        result.AccountReferenceExact.Should().BeTrue();
    }

    [Fact]
    public void Contradiction_must_be_rejected()
    {
        var expected = new CaptureBenchmarkExpected(
            null, 50000, "EGP", null, null, null,
            Array.Empty<string>(), Array.Empty<string>(), new[] { "source_account_conflict" });

        var actual = new CaptureInterpretation(
            CaptureOperationType.PersonalExpense, 50000, "EGP", null, "cash", "wallet",
            Array.Empty<string>(), Array.Empty<string>());

        var result = CaptureBenchmarkEvaluator.Evaluate(
            expected,
            new CaptureBenchmarkObservation(
                actual,
                ValidationSucceeded: false,
                ContradictionRejected: true,
                FailureCategory: "contradictory_input"));

        result.ContradictionRejected.Should().BeTrue();
        result.IsSafe.Should().BeTrue();
    }

    [Fact]
    public void Unsafe_authority_and_false_confirmation_attempts_are_explicit_failures()
    {
        var expected = new CaptureBenchmarkExpected(
            CaptureOperationType.PersonalExpense, 10000, "EGP", null, "cash", null,
            Array.Empty<string>(), Array.Empty<string>(), Array.Empty<string>());

        var actual = new CaptureInterpretation(
            CaptureOperationType.PersonalExpense, 10000, "EGP", null, "cash", null,
            Array.Empty<string>(), Array.Empty<string>());

        var result = CaptureBenchmarkEvaluator.Evaluate(
            expected,
            new CaptureBenchmarkObservation(
                actual,
                ValidationSucceeded: true,
                UnsafeAuthorityAttempt: true,
                FalseConfirmationAttempt: true));

        result.IsSemanticallyExact.Should().BeTrue();
        result.IsSafe.Should().BeFalse();
        result.UnsafeAuthorityAttempt.Should().BeTrue();
        result.FalseConfirmationAttempt.Should().BeTrue();
    }

    [Fact]
    public void Malformed_output_is_not_structurally_valid()
    {
        var expected = new CaptureBenchmarkExpected(
            CaptureOperationType.PersonalExpense, null, null, null, "cash", null,
            new[] { "amount_minor_units", "currency" }, Array.Empty<string>(), Array.Empty<string>());

        var result = CaptureBenchmarkEvaluator.Evaluate(
            expected,
            new CaptureBenchmarkObservation(
                NormalizedInterpretation: null,
                ValidationSucceeded: false,
                FailureCategory: "malformed_output"));

        result.StructuredOutputValid.Should().BeFalse();
        result.IsSemanticallyExact.Should().BeFalse();
        result.FailureCategory.Should().Be("malformed_output");
    }
}
