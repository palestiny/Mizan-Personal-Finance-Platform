using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Json;
using Xunit;
using Mizan.Domain.Finance;

namespace Mizan.Api.Tests;

public sealed class ProposalApiTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    public ProposalApiTests(WebApplicationFactory<Program> factory)=>_client=factory.CreateClient();

    [Fact]
    public async Task Api_should_create_inspect_and_confirm_income_proposal()
    {
        var account=await CreateAccountAsync("Proposal Income");
        var create=await _client.PostAsJsonAsync("/api/proposals",new
        {
            originalInput="استلمت 8000 جنيه مرتب في البنك",
            operationType="Income",
            accountId=account,
            amountMinorUnits=800000,
            currency="EGP",
            effectiveAt="2026-09-22T10:00:00+03:00",
            expiresAt="2026-09-23T10:00:00+03:00"
        });
        create.StatusCode.Should().Be(HttpStatusCode.Created);
        var proposal=await create.Content.ReadFromJsonAsync<ProposalApiResponse>();
        proposal!.Status.Should().Be("ReadyForConfirmation");
        proposal.OperationId.Should().BeNull();

        var confirm=await _client.PostAsJsonAsync($"/api/proposals/{proposal.Id}/confirm",new { commandIdempotencyKey="proposal-income-confirm-1" });
        confirm.StatusCode.Should().Be(HttpStatusCode.OK);
        var operation=await confirm.Content.ReadFromJsonAsync<OperationResponse>();
        operation!.Type.Should().Be(FinancialOperationType.Income);

        var read=await _client.GetFromJsonAsync<ProposalApiResponse>($"/api/proposals/{proposal.Id}");
        read!.Status.Should().Be("Confirmed");
        read.OperationId.Should().Be(operation.Id);
    }

    [Fact]
    public async Task Api_should_not_create_financial_state_when_proposal_is_rejected()
    {
        var account=await CreateAccountAsync("Proposal Reject",1000);
        var create=await _client.PostAsJsonAsync("/api/proposals",new
        {
            originalInput="دفع 500 جنيه",
            operationType="PersonalExpense",
            accountId=account,
            amountMinorUnits=50000,
            currency="EGP",
            effectiveAt="2026-09-22T10:00:00+03:00",
            expiresAt="2026-09-23T10:00:00+03:00"
        });
        var proposal=await create.Content.ReadFromJsonAsync<ProposalApiResponse>();
        (await _client.PostAsync($"/api/proposals/{proposal!.Id}/reject",null)).StatusCode.Should().Be(HttpStatusCode.OK);
        var confirm=await _client.PostAsJsonAsync($"/api/proposals/{proposal.Id}/confirm",new { commandIdempotencyKey="proposal-reject-confirm" });
        confirm.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var balance=await _client.GetFromJsonAsync<BalanceResponse>($"/api/accounts/{account}/balance");
        balance!.AmountMinorUnits.Should().Be(1000);
    }

    [Fact]
    public async Task Api_should_allow_only_one_financial_operation_for_concurrent_proposal_confirmation()
    {
        var account=await CreateAccountAsync("Proposal Race");
        var create=await _client.PostAsJsonAsync("/api/proposals",new
        {
            originalInput="استلمت 100 جنيه",
            operationType="Income",
            accountId=account,
            amountMinorUnits=10000,
            currency="EGP",
            effectiveAt="2026-09-22T10:00:00+03:00",
            expiresAt="2026-09-23T10:00:00+03:00"
        });
        var proposal=await create.Content.ReadFromJsonAsync<ProposalApiResponse>();

        var responses=await Task.WhenAll(
            _client.PostAsJsonAsync($"/api/proposals/{proposal!.Id}/confirm",new { commandIdempotencyKey="proposal-race-a" }),
            _client.PostAsJsonAsync($"/api/proposals/{proposal.Id}/confirm",new { commandIdempotencyKey="proposal-race-b" }));

        responses.Select(x=>x.StatusCode).Should().Contain(HttpStatusCode.OK);
        responses.Select(x=>x.StatusCode).Should().Contain(HttpStatusCode.Conflict);
        var balance=await _client.GetFromJsonAsync<BalanceResponse>($"/api/accounts/{account}/balance");
        balance!.AmountMinorUnits.Should().Be(10000);
    }

    [Fact]
    public async Task Api_should_keep_draft_when_required_fields_are_missing()
    {
        var create=await _client.PostAsJsonAsync("/api/proposals",new
        {
            originalInput="دفعت بنزين",
            operationType="PersonalExpense",
            missingFields="Amount,Account",
            expiresAt="2026-09-23T10:00:00+03:00"
        });
        create.StatusCode.Should().Be(HttpStatusCode.Created);
        var proposal=await create.Content.ReadFromJsonAsync<ProposalApiResponse>();
        proposal!.Status.Should().Be("Draft");
        var confirm=await _client.PostAsJsonAsync($"/api/proposals/{proposal.Id}/confirm",new { commandIdempotencyKey="proposal-draft-confirm" });
        confirm.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    private async Task<Guid> CreateAccountAsync(string name,long opening=0)
    {
        var response=await _client.PostAsJsonAsync("/api/accounts",new { name,type="Cash",currency="EGP",openingBalanceMinorUnits=opening });
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var account=await response.Content.ReadFromJsonAsync<AccountApiResponse>();
        return account!.Id;
    }
}

public sealed record ProposalApiResponse(Guid Id,string OriginalInput,string OperationType,Guid? AccountId,Guid? DestinationAccountId,long? AmountMinorUnits,string? Currency,DateTimeOffset? EffectiveAt,string? MissingFields,string? Ambiguities,string? InterpretationMetadata,string Status,DateTimeOffset CreatedAt,DateTimeOffset ExpiresAt,DateTimeOffset? ConfirmedAt,DateTimeOffset? RejectedAt,string? CommandIdempotencyKey,Guid? OperationId);
public sealed record AccountApiResponse(Guid Id,string Name,string Type,string Currency,string Status);

public sealed record BalanceApiResponse(long AmountMinorUnits, string Status);
