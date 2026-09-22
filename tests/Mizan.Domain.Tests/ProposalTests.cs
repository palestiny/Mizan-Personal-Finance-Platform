using FluentAssertions;
using Mizan.Application.Capture;
using Mizan.Domain.Finance;
using Xunit;

namespace Mizan.Domain.Tests;

public sealed class ProposalTests
{
    [Fact]
    public void Complete_proposal_starts_ready_for_confirmation()
    {
        var account=Guid.NewGuid();
        var p=Proposal.Create("دفع 250 جنيه",ProposalOperationType.PersonalExpense,account,null,25000,"EGP",DateTimeOffset.UtcNow.AddMinutes(1),null,null,null,DateTimeOffset.UtcNow.AddHours(1));
        p.Status.Should().Be(ProposalStatus.ReadyForConfirmation);
    }

    [Fact]
    public void Missing_fields_keep_proposal_non_authoritative_and_draft()
    {
        var p=Proposal.Create("دفع بنزين",ProposalOperationType.PersonalExpense,null,null,null,null,null,"Amount,Account",null,null,DateTimeOffset.UtcNow.AddHours(1));
        p.Status.Should().Be(ProposalStatus.Draft);
        var action=()=>p.Prepare();
        action.Should().Throw<DomainValidationException>();
    }

    [Fact]
    public void Confirmation_key_cannot_change_after_association()
    {
        var p=Proposal.Create("دخل 8000",ProposalOperationType.Income,Guid.NewGuid(),null,800000,"EGP",DateTimeOffset.UtcNow.AddMinutes(1),null,null,null,DateTimeOffset.UtcNow.AddHours(1));
        p.AssociateConfirmation("proposal-command-1");
        var action=()=>p.AssociateConfirmation("proposal-command-2");
        action.Should().Throw<IdempotencyConflictException>();
    }

    [Fact]
    public void Rejected_proposal_cannot_be_confirmed()
    {
        var p=Proposal.Create("دفع 250",ProposalOperationType.PersonalExpense,Guid.NewGuid(),null,25000,"EGP",DateTimeOffset.UtcNow.AddMinutes(1),null,null,null,DateTimeOffset.UtcNow.AddHours(1));
        p.Reject();
        var action=()=>p.AssociateConfirmation("proposal-command");
        action.Should().Throw<DomainValidationException>();
    }
}
