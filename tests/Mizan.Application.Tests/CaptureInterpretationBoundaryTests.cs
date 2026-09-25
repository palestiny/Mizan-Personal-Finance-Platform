using FluentAssertions;
using Mizan.Application.Capture;
using Mizan.Domain.Finance;
using Xunit;

namespace Mizan.Application.Tests;

public sealed class CaptureInterpretationBoundaryTests
{
    [Fact]
    public void Interpreter_returns_vendor_neutral_semantics_without_internal_account_ids()
    {
        var interpreter = new DeterministicCaptureInterpreter();

        var result = interpreter.Interpret(new CaptureInput("دفعت 250 جنيه للبنزين من البنك", "text", null, null));

        result.OperationType.Should().Be(CaptureOperationType.PersonalExpense);
        result.AmountMinorUnits.Should().Be(25000);
        result.Currency.Should().Be("EGP");
        result.AccountReference.Should().Be("البنك");
        result.AccountId.Should().BeNull();
    }

    [Fact]
    public void Resolver_resolves_unique_semantic_account_reference()
    {
        var accountId = Guid.NewGuid();
        var resolver = new DeterministicCaptureContextResolver(
            new Dictionary<string, Guid>(StringComparer.OrdinalIgnoreCase)
            {
                ["البنك"] = accountId
            });

        var result = resolver.Resolve(new CaptureInterpretation(
            CaptureOperationType.PersonalExpense, 25000, "EGP", null, "البنك", null, null));

        result.AccountId.Should().Be(accountId);
        result.IsComplete.Should().BeTrue();
    }

    [Fact]
    public void Resolver_marks_unknown_reference_as_missing_instead_of_guessing()
    {
        var resolver = new DeterministicCaptureContextResolver(new Dictionary<string, Guid>());

        var result = resolver.Resolve(new CaptureInterpretation(
            CaptureOperationType.PersonalExpense, 25000, "EGP", null, "البنك", null, null));

        result.AccountId.Should().BeNull();
        result.MissingFields.Should().Contain("Account");
        result.IsComplete.Should().BeFalse();
    }

    [Fact]
    public void Resolver_marks_ambiguous_reference_without_selecting_an_account()
    {
        var first = Guid.NewGuid();
        var second = Guid.NewGuid();
        var resolver = new DeterministicCaptureContextResolver(
            new Dictionary<string, IReadOnlyList<Guid>>(StringComparer.OrdinalIgnoreCase)
            {
                ["البنك"] = [first, second]
            });

        var result = resolver.Resolve(new CaptureInterpretation(
            CaptureOperationType.PersonalExpense, 25000, "EGP", null, "البنك", null, null));

        result.AccountId.Should().BeNull();
        result.Ambiguities.Should().Contain("Account");
        result.IsComplete.Should().BeFalse();
    }

    [Fact]
    public void Incomplete_interpretation_cannot_be_materialized_as_confirmable_proposal()
    {
        var interpreter = new DeterministicCaptureInterpreter();
        var resolver = new DeterministicCaptureContextResolver(new Dictionary<string, Guid>());
        var service = new CaptureProposalService(interpreter, resolver);

        var proposal = service.CreateProposal(
            new CaptureInput("دفعت بنزين", "text", null, null),
            DateTimeOffset.UtcNow.AddHours(1));

        proposal.Status.Should().Be(ProposalStatus.Draft);
        proposal.MissingFields.Should().NotBeNullOrWhiteSpace();
    }
}
