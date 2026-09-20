using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Json;

namespace Mizan.Api.Tests;

public sealed class FinancialFlowApiTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public FinancialFlowApiTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Api_should_expose_one_complete_financial_flow()
    {
        var createAccount = await _client.PostAsJsonAsync("/api/accounts", new
        {
            name = "Cash",
            type = "Cash",
            currency = "EGP"
        });
        createAccount.StatusCode.Should().Be(HttpStatusCode.Created);

        var account = await createAccount.Content.ReadFromJsonAsync<AccountResponse>();
        account.Should().NotBeNull();

        var income = await _client.PostAsJsonAsync("/api/operations/income", new
        {
            accountId = account!.Id,
            amountMinorUnits = 10000,
            currency = "EGP",
            effectiveAt = "2026-09-20T10:00:00+03:00",
            idempotencyKey = "api-flow-1"
        });
        income.StatusCode.Should().Be(HttpStatusCode.OK);

        var balance = await _client.GetFromJsonAsync<BalanceResponse>($"/api/accounts/{account.Id}/balance");
        balance!.AmountMinorUnits.Should().Be(10000);
    }

    private sealed record AccountResponse(Guid Id);
    private sealed record BalanceResponse(long AmountMinorUnits);
}
