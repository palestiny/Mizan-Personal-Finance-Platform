using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace Mizan.Api.Tests;

public sealed class FinancialFlowApiTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public FinancialFlowApiTests(WebApplicationFactory<Program> factory) => _client = factory.CreateClient();

    [Fact]
    public async Task Api_should_accept_income()
    {
        var account = await CreateAccountAsync("Cash Income");
        var response = await _client.PostAsJsonAsync("/api/operations/income", new
        {
            accountId = account.Id, amountMinorUnits = 10000, currency = "EGP",
            effectiveAt = "2026-09-20T10:00:00+03:00", idempotencyKey = "api-income-1"
        });
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Api_should_return_same_operation_for_idempotent_retry()
    {
        var account = await CreateAccountAsync("Cash Retry");
        var request = new
        {
            accountId = account.Id, amountMinorUnits = 10000, currency = "EGP",
            effectiveAt = "2026-09-20T10:00:00+03:00", idempotencyKey = "api-retry-1"
        };
        var first = await _client.PostAsJsonAsync("/api/operations/income", request);
        var second = await _client.PostAsJsonAsync("/api/operations/income", request);
        first.StatusCode.Should().Be(HttpStatusCode.OK);
        second.StatusCode.Should().Be(HttpStatusCode.OK);
        (await second.Content.ReadFromJsonAsync<OperationResponse>())!.Id
            .Should().Be((await first.Content.ReadFromJsonAsync<OperationResponse>())!.Id);
    }

    [Fact]
    public async Task Api_should_reject_different_command_under_same_idempotency_key()
    {
        var account = await CreateAccountAsync("Cash Conflict");
        var first = await _client.PostAsJsonAsync("/api/operations/income", new
        {
            accountId = account.Id, amountMinorUnits = 10000, currency = "EGP",
            effectiveAt = "2026-09-20T10:00:00+03:00", idempotencyKey = "api-conflict-1"
        });
        first.StatusCode.Should().Be(HttpStatusCode.OK);

        var conflict = await _client.PostAsJsonAsync("/api/operations/income", new
        {
            accountId = account.Id, amountMinorUnits = 11000, currency = "EGP",
            effectiveAt = "2026-09-20T10:00:00+03:00", idempotencyKey = "api-conflict-1"
        });
        conflict.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Api_should_protect_concurrent_duplicate_income()
    {
        var account = await CreateAccountAsync("Cash Concurrent");
        var request = new { accountId = account.Id, amountMinorUnits = 5000, currency = "EGP", effectiveAt = "2026-09-20T11:00:00+03:00", idempotencyKey = "api-concurrent-1" };

        var responses = await Task.WhenAll(
            _client.PostAsJsonAsync("/api/operations/income", request),
            _client.PostAsJsonAsync("/api/operations/income", request));

        responses.Should().OnlyContain(x => x.StatusCode == HttpStatusCode.OK);
        var operations = await Task.WhenAll(responses.Select(x => x.Content.ReadFromJsonAsync<OperationResponse>()));
        operations.Select(x => x!.Id).Distinct().Should().ContainSingle();
    }

    [Fact]
    public async Task Api_should_derive_balance_after_income_and_concurrent_retry()
    {
        var account = await CreateAccountAsync("Cash Balance");
        var income = await _client.PostAsJsonAsync("/api/operations/income", new { accountId = account.Id, amountMinorUnits = 10000, currency = "EGP", effectiveAt = "2026-09-20T10:00:00+03:00", idempotencyKey = "api-balance-1" });
        income.StatusCode.Should().Be(HttpStatusCode.OK);

        var duplicate = await _client.PostAsJsonAsync("/api/operations/income", new { accountId = account.Id, amountMinorUnits = 5000, currency = "EGP", effectiveAt = "2026-09-20T11:00:00+03:00", idempotencyKey = "api-balance-2" });
        duplicate.StatusCode.Should().Be(HttpStatusCode.OK);

        var balance = await _client.GetFromJsonAsync<BalanceResponse>($"/api/accounts/{account.Id}/balance");
        balance!.AmountMinorUnits.Should().Be(15000);
    }

    [Fact]
    public async Task Api_should_reverse_an_accepted_operation_without_mutating_the_original()
    {
        var account = await CreateAccountAsync("Reversal Cash", 1000);

        var income = await _client.PostAsJsonAsync("/api/operations/income", new
        {
            accountId = account.Id,
            amountMinorUnits = 500,
            currency = "EGP",
            effectiveAt = "2026-09-20T10:00:00+03:00",
            idempotencyKey = "reversal-original-1"
        });
        var original = await income.Content.ReadFromJsonAsync<OperationResponse>();

        var reversal = await _client.PostAsJsonAsync("/api/operations/reversal", new
        {
            originalOperationId = original!.Id,
            effectiveAt = "2026-09-21T10:00:00+03:00",
            idempotencyKey = "reversal-command-1"
        });
        reversal.StatusCode.Should().Be(HttpStatusCode.OK);
        var reversed = await reversal.Content.ReadFromJsonAsync<OperationResponse>();

        reversed!.Type.Should().Be("Reversal");
        reversed.OriginalOperationId.Should().Be(original.Id);
        reversed.Effects.Should().ContainSingle();

        var duplicate = await _client.PostAsJsonAsync("/api/operations/reversal", new
        {
            originalOperationId = original.Id,
            effectiveAt = "2026-09-21T10:00:00+03:00",
            idempotencyKey = "reversal-command-1"
        });
        duplicate.StatusCode.Should().Be(HttpStatusCode.OK);
        (await duplicate.Content.ReadFromJsonAsync<OperationResponse>())!.Id.Should().Be(reversed.Id);

        var second = await _client.PostAsJsonAsync("/api/operations/reversal", new
        {
            originalOperationId = original.Id,
            effectiveAt = "2026-09-22T10:00:00+03:00",
            idempotencyKey = "reversal-command-2"
        });
        second.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var balance = await _client.GetFromJsonAsync<BalanceResponse>($"/api/accounts/{account.Id}/balance");
        balance!.AmountMinorUnits.Should().Be(1000);
    }

    private async Task<AccountResponse> CreateAccountAsync(string name, long? openingBalanceMinorUnits = null)
    {
        var response = await _client.PostAsJsonAsync("/api/accounts", new { name, type = "Cash", currency = "EGP", openingBalanceMinorUnits });
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        return (await response.Content.ReadFromJsonAsync<AccountResponse>())!;
    }

    private sealed record AccountResponse(Guid Id);
    private sealed record BalanceResponse(long AmountMinorUnits);
    private sealed record OperationResponse(Guid Id, string Type, Guid? OriginalOperationId, IReadOnlyList<EffectResponse> Effects);
    private sealed record EffectResponse(Guid Id, Guid AccountId, long AmountMinorUnits, string Currency, string Direction, long Order);
}
