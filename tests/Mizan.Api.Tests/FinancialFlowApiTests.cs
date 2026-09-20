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
    public async Task Api_should_expose_one_complete_financial_flow()
    {
        var createAccount = await _client.PostAsJsonAsync("/api/accounts", new { name = "Cash", type = "Cash", currency = "EGP" });
        createAccount.StatusCode.Should().Be(HttpStatusCode.Created);
        var account = await createAccount.Content.ReadFromJsonAsync<AccountResponse>();
        account.Should().NotBeNull();

        var incomeRequest = new { accountId = account!.Id, amountMinorUnits = 10000, currency = "EGP", effectiveAt = "2026-09-20T10:00:00+03:00", idempotencyKey = "api-flow-1" };
        var income = await _client.PostAsJsonAsync("/api/operations/income", incomeRequest);
        income.StatusCode.Should().Be(HttpStatusCode.OK);
        var duplicate = await _client.PostAsJsonAsync("/api/operations/income", incomeRequest);
        duplicate.StatusCode.Should().Be(HttpStatusCode.OK);

        var firstOperation = await income.Content.ReadFromJsonAsync<OperationResponse>();
        var duplicateOperation = await duplicate.Content.ReadFromJsonAsync<OperationResponse>();
        duplicateOperation!.Id.Should().Be(firstOperation!.Id);

        var conflict = await _client.PostAsJsonAsync("/api/operations/income", incomeRequest with { amountMinorUnits = 11000 });
        conflict.StatusCode.Should().Be(HttpStatusCode.Conflict);

        var concurrentRequests = Enumerable.Range(0, 2).Select(_ => _client.PostAsJsonAsync("/api/operations/income", new
        {
            accountId = account.Id, amountMinorUnits = 5000, currency = "EGP", effectiveAt = "2026-09-20T11:00:00+03:00", idempotencyKey = "api-concurrent-1"
        })).ToArray();
        var concurrentResponses = await Task.WhenAll(concurrentRequests);
        concurrentResponses.Should().OnlyContain(x => x.StatusCode == HttpStatusCode.OK);
        var concurrentOperations = await Task.WhenAll(concurrentResponses.Select(x => x.Content.ReadFromJsonAsync<OperationResponse>()));
        concurrentOperations.Select(x => x!.Id).Distinct().Should().ContainSingle();

        var balance = await _client.GetFromJsonAsync<BalanceResponse>($"/api/accounts/{account.Id}/balance");
        balance!.AmountMinorUnits.Should().Be(15000);
    }

    private sealed record AccountResponse(Guid Id);
    private sealed record BalanceResponse(long AmountMinorUnits);
    private sealed record OperationResponse(Guid Id);
}
