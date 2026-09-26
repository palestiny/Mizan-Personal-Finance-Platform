using FluentAssertions;
using Mizan.Application.Capture;
using Xunit;

namespace Mizan.Application.Tests;

public sealed class ProviderCaptureProposalServiceTests
{
    [Fact]
    public async Task Valid_provider_output_creates_proposal_but_no_financial_state()
    {
        var accountId = Guid.NewGuid();
        var provider = new FakeProvider(CaptureProviderResult.Success(
            new CaptureInterpretation(
                CaptureOperationType.PersonalExpense,
                25000,
                "egp",
                DateTimeOffset.UtcNow,
                "البنك",
                null,
                null)));
        var resolver = new DeterministicCaptureContextResolver(
            new Dictionary<string, Guid>(StringComparer.OrdinalIgnoreCase)
            {
                ["البنك"] = accountId
            });

        var service = new ProviderCaptureProposalService(provider, resolver);

        var result = await service.CreateProposalAsync(
            new CaptureInput("دفعت 250 جنيه للبنزين من البنك", "text", "ar-EG", "Africa/Cairo"),
            DateTimeOffset.UtcNow.AddHours(1));

        result.Succeeded.Should().BeTrue();
        result.Proposal.Should().NotBeNull();
        result.Proposal!.Status.Should().Be(ProposalStatus.ReadyForConfirmation);
        result.Proposal.AccountId.Should().Be(accountId);
        result.Proposal.OperationId.Should().BeNull();
    }

    [Fact]
    public async Task Unknown_semantic_reference_remains_draft_and_unresolved()
    {
        var provider = new FakeProvider(CaptureProviderResult.Success(
            new CaptureInterpretation(
                CaptureOperationType.PersonalExpense,
                25000,
                "EGP",
                DateTimeOffset.UtcNow,
                "unknown-account",
                null,
                null)));
        var resolver = new DeterministicCaptureContextResolver(new Dictionary<string, Guid>());
        var service = new ProviderCaptureProposalService(provider, resolver);

        var result = await service.CreateProposalAsync(
            new CaptureInput("expense", "text", null, null),
            DateTimeOffset.UtcNow.AddHours(1));

        result.Succeeded.Should().BeTrue();
        result.Proposal!.Status.Should().Be(ProposalStatus.Draft);
        result.Proposal.AccountId.Should().BeNull();
        result.Proposal.MissingFields.Should().Contain("Account");
        result.Proposal.OperationId.Should().BeNull();
    }

    [Fact]
    public async Task Ambiguous_semantic_reference_remains_draft_without_selection()
    {
        var first = Guid.NewGuid();
        var second = Guid.NewGuid();
        var provider = new FakeProvider(CaptureProviderResult.Success(
            new CaptureInterpretation(
                CaptureOperationType.PersonalExpense,
                25000,
                "EGP",
                DateTimeOffset.UtcNow,
                "البنك",
                null,
                null)));
        var resolver = new DeterministicCaptureContextResolver(
            new Dictionary<string, IReadOnlyList<Guid>>(StringComparer.OrdinalIgnoreCase)
            {
                ["البنك"] = [first, second]
            });
        var service = new ProviderCaptureProposalService(provider, resolver);

        var result = await service.CreateProposalAsync(
            new CaptureInput("expense", "text", null, null),
            DateTimeOffset.UtcNow.AddHours(1));

        result.Succeeded.Should().BeTrue();
        result.Proposal!.Status.Should().Be(ProposalStatus.Draft);
        result.Proposal.AccountId.Should().BeNull();
        result.Proposal.Ambiguities.Should().Contain("Account");
    }

    [Fact]
    public async Task Provider_failure_creates_no_proposal()
    {
        var provider = new FakeProvider(CaptureProviderResult.Failure("provider unavailable"));
        var resolver = new DeterministicCaptureContextResolver(new Dictionary<string, Guid>());
        var service = new ProviderCaptureProposalService(provider, resolver);

        var result = await service.CreateProposalAsync(
            new CaptureInput("expense", "text", null, null),
            DateTimeOffset.UtcNow.AddHours(1));

        result.Succeeded.Should().BeFalse();
        result.Proposal.Should().BeNull();
        result.FailureReason.Should().Be("provider unavailable");
    }

    [Fact]
    public async Task Invalid_provider_output_creates_no_proposal()
    {
        var provider = new FakeProvider(CaptureProviderResult.Success(
            new CaptureInterpretation(
                CaptureOperationType.PersonalExpense,
                0,
                "EGP",
                DateTimeOffset.UtcNow,
                "البنك",
                null,
                null)));
        var resolver = new DeterministicCaptureContextResolver(new Dictionary<string, Guid>());
        var service = new ProviderCaptureProposalService(provider, resolver);

        var result = await service.CreateProposalAsync(
            new CaptureInput("expense", "text", null, null),
            DateTimeOffset.UtcNow.AddHours(1));

        result.Succeeded.Should().BeFalse();
        result.Proposal.Should().BeNull();
        result.FailureReason.Should().Contain("invalid amount");
    }

    private sealed class FakeProvider(CaptureProviderResult result) : ICaptureProviderAdapter
    {
        public Task<CaptureProviderResult> InterpretAsync(
            CaptureInput input,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(result);
    }
}
