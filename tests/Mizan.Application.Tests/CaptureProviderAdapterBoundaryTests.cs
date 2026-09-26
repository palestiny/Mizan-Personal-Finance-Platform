using FluentAssertions;
using Mizan.Application.Capture;
using Xunit;

namespace Mizan.Application.Tests;

public sealed class CaptureProviderAdapterBoundaryTests
{
    [Fact]
    public void Provider_result_contains_no_authoritative_internal_account_identity()
    {
        var interpretation = new CaptureInterpretation(
            CaptureOperationType.PersonalExpense,
            25000,
            "egp",
            null,
            "البنك",
            "المحفظة",
            null,
            null);

        var result = CaptureProviderResult.Success(interpretation);

        result.Interpretation.Should().NotBeNull();
        result.Interpretation!.AccountReference.Should().Be("البنك");
        result.Interpretation.DestinationAccountReference.Should().Be("المحفظة");
        result.Interpretation.Should().BeOfType<CaptureInterpretation>();
    }

    [Fact]
    public void Valid_provider_result_is_normalized_without_changing_financial_semantics()
    {
        var result = CaptureProviderResult.Success(
            new CaptureInterpretation(
                CaptureOperationType.PersonalExpense,
                25000,
                " egp ",
                null,
                " البنك ",
                " المحفظة ",
                new[] { "Amount", " Amount " },
                new[] { "Account", "Account" }),
            new CaptureProviderDiagnostics(0.82m, new[] { "  uncertain vendor  ", "uncertain vendor" }));

        var valid = CaptureProviderResultValidator.TryValidate(
            result,
            out var validated,
            out var error);

        valid.Should().BeTrue();
        error.Should().BeEmpty();
        validated.Interpretation!.Currency.Should().Be("EGP");
        validated.Interpretation.AccountReference.Should().Be("البنك");
        validated.Interpretation.DestinationAccountReference.Should().Be("المحفظة");
        validated.Interpretation.MissingFields.Should().Equal("Amount");
        validated.Interpretation.Ambiguities.Should().Equal("Account");
        validated.Diagnostics.Warnings.Should().Equal("uncertain vendor");
        validated.Diagnostics.Confidence.Should().Be(0.82m);
    }

    [Fact]
    public void Invalid_provider_amount_fails_closed()
    {
        var result = CaptureProviderResult.Success(
            new CaptureInterpretation(
                CaptureOperationType.PersonalExpense,
                0,
                "EGP",
                null,
                "البنك",
                null,
                null,
                null));

        var valid = CaptureProviderResultValidator.TryValidate(
            result,
            out var validated,
            out var error);

        valid.Should().BeFalse();
        validated.Succeeded.Should().BeFalse();
        validated.Interpretation.Should().BeNull();
        error.Should().Contain("invalid amount");
    }

    [Fact]
    public void Invalid_provider_confidence_fails_closed()
    {
        var result = CaptureProviderResult.Success(
            new CaptureInterpretation(
                CaptureOperationType.PersonalExpense,
                25000,
                "EGP",
                null,
                "البنك",
                null,
                null,
                null),
            new CaptureProviderDiagnostics(1.1m, Array.Empty<string>()));

        var valid = CaptureProviderResultValidator.TryValidate(
            result,
            out var validated,
            out var error);

        valid.Should().BeFalse();
        validated.Succeeded.Should().BeFalse();
        error.Should().Contain("confidence");
    }

    [Fact]
    public void Provider_failure_is_not_treated_as_a_partial_financial_interpretation()
    {
        var result = CaptureProviderResult.Failure("provider timeout");

        var valid = CaptureProviderResultValidator.TryValidate(
            result,
            out var validated,
            out var error);

        valid.Should().BeFalse();
        validated.Succeeded.Should().BeFalse();
        validated.Interpretation.Should().BeNull();
        validated.FailureReason.Should().Be("provider timeout");
        error.Should().BeEmpty();
    }
}
