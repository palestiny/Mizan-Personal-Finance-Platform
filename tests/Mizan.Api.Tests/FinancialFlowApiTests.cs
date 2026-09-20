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
    public async Task Api_should_close_and_reopen_account()
    {
        var account = await CreateAccountAsync("Lifecycle Cash");

        var close = await _client.PostAsync($"/api/accounts/{account.Id}/close", null);
        close.StatusCode.Should().Be(HttpStatusCode.OK);
        var closed = await close.Content.ReadFromJsonAsync<AccountLifecycleResponse>();
        closed!.Status.Should().Be("Closed");

        var reopen = await _client.PostAsync($"/api/accounts/{account.Id}/reopen", null);
        reopen.StatusCode.Should().Be(HttpStatusCode.OK);
        var active = await reopen.Content.ReadFromJsonAsync<AccountLifecycleResponse>();
        active!.Status.Should().Be("Active");
    }

    [Fact]
    public async Task Api_should_reject_normal_operations_on_closed_account_but_keep_balance_readable()
    {
        var account = await CreateAccountAsync("Closed Cash", 1000);
        var close = await _client.PostAsync($"/api/accounts/{account.Id}/close", null);
        close.StatusCode.Should().Be(HttpStatusCode.OK);

        var income = await _client.PostAsJsonAsync("/api/operations/income", new
        {
            accountId = account.Id, amountMinorUnits = 500, currency = "EGP",
            effectiveAt = "2026-09-21T10:00:00+03:00", idempotencyKey = "closed-income-1"
        });
        income.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var balance = await _client.GetFromJsonAsync<BalanceResponse>($"/api/accounts/{account.Id}/balance");
        balance!.AmountMinorUnits.Should().Be(1000);
        balance.Status.Should().Be("Closed");
    }

    [Fact]
    public async Task Api_should_allow_reversal_after_account_is_closed()
    {
        var account = await CreateAccountAsync("Closed Reversal", 1000);
        var income = await _client.PostAsJsonAsync("/api/operations/income", new
        {
            accountId = account.Id, amountMinorUnits = 500, currency = "EGP",
            effectiveAt = "2026-09-21T10:00:00+03:00", idempotencyKey = "closed-reversal-original"
        });
        var original = await income.Content.ReadFromJsonAsync<OperationResponse>();

        (await _client.PostAsync($"/api/accounts/{account.Id}/close", null)).StatusCode.Should().Be(HttpStatusCode.OK);

        var reversal = await _client.PostAsJsonAsync("/api/operations/reversal", new
        {
            originalOperationId = original!.Id,
            effectiveAt = "2026-09-21T11:00:00+03:00",
            idempotencyKey = "closed-reversal-command"
        });

        reversal.StatusCode.Should().Be(HttpStatusCode.OK);
        var balance = await _client.GetFromJsonAsync<BalanceResponse>($"/api/accounts/{account.Id}/balance");
        balance!.AmountMinorUnits.Should().Be(1000);
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


    [Fact]
    public async Task Api_should_create_and_settle_recoverable_partially()
    {
        var account = await CreateAccountAsync("Recoverable Cash", 5000);

        var created = await _client.PostAsJsonAsync("/api/operations/recoverable-expense", new
        {
            accountId = account.Id,
            amountMinorUnits = 1000,
            currency = "EGP",
            counterpartyName = "Ahmed",
            effectiveAt = "2026-09-21T10:00:00+03:00",
            idempotencyKey = "recoverable-expense-1"
        });
        created.StatusCode.Should().Be(HttpStatusCode.OK);
        var original = await created.Content.ReadFromJsonAsync<OperationResponse>();
        original!.Type.Should().Be("RecoverableExpense");
        original.RecoverableEffects.Should().ContainSingle();
        var recoverableId = original.RecoverableEffects[0].RecoverableId;

        var afterExpense = await _client.GetFromJsonAsync<BalanceResponse>($"/api/accounts/{account.Id}/balance");
        afterExpense!.AmountMinorUnits.Should().Be(4000);

        var settlement = await _client.PostAsJsonAsync("/api/operations/recoverable-settlement", new
        {
            accountId = account.Id,
            recoverableId,
            amountMinorUnits = 400,
            currency = "EGP",
            effectiveAt = "2026-09-22T10:00:00+03:00",
            idempotencyKey = "recoverable-settlement-1"
        });
        settlement.StatusCode.Should().Be(HttpStatusCode.OK);

        var recoverable = await _client.GetFromJsonAsync<RecoverableResponse>($"/api/recoverables/{recoverableId}");
        recoverable!.OutstandingMinorUnits.Should().Be(600);
        recoverable.Status.Should().Be("Outstanding");

        var afterSettlement = await _client.GetFromJsonAsync<BalanceResponse>($"/api/accounts/{account.Id}/balance");
        afterSettlement!.AmountMinorUnits.Should().Be(4400);
    }

    [Fact]
    public async Task Api_should_reject_recoverable_over_settlement()
    {
        var account = await CreateAccountAsync("Recoverable Over", 5000);
        var created = await _client.PostAsJsonAsync("/api/operations/recoverable-expense", new
        {
            accountId = account.Id, amountMinorUnits = 1000, currency = "EGP",
            counterpartyName = "Ahmed", effectiveAt = "2026-09-21T10:00:00+03:00", idempotencyKey = "recoverable-over-1"
        });
        var original = await created.Content.ReadFromJsonAsync<OperationResponse>();
        var recoverableId = original!.RecoverableEffects[0].RecoverableId;

        var settlement = await _client.PostAsJsonAsync("/api/operations/recoverable-settlement", new
        {
            accountId = account.Id, recoverableId, amountMinorUnits = 1001, currency = "EGP",
            effectiveAt = "2026-09-22T10:00:00+03:00", idempotencyKey = "recoverable-over-2"
        });

        settlement.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Api_should_allow_reversal_of_recoverable_expense()
    {
        var account = await CreateAccountAsync("Recoverable Reversal", 5000);
        var created = await _client.PostAsJsonAsync("/api/operations/recoverable-expense", new
        {
            accountId = account.Id, amountMinorUnits = 1000, currency = "EGP",
            counterpartyName = "Ahmed", effectiveAt = "2026-09-21T10:00:00+03:00", idempotencyKey = "recoverable-reversal-1"
        });
        var original = await created.Content.ReadFromJsonAsync<OperationResponse>();

        var reversal = await _client.PostAsJsonAsync("/api/operations/reversal", new
        {
            originalOperationId = original!.Id, effectiveAt = "2026-09-22T10:00:00+03:00", idempotencyKey = "recoverable-reversal-2"
        });
        reversal.StatusCode.Should().Be(HttpStatusCode.OK);

        var recoverable = await _client.GetFromJsonAsync<RecoverableResponse>($"/api/recoverables/{original.RecoverableEffects[0].RecoverableId}");
        recoverable!.OutstandingMinorUnits.Should().Be(0);
        recoverable.Status.Should().Be("Settled");

        var balance = await _client.GetFromJsonAsync<BalanceResponse>($"/api/accounts/{account.Id}/balance");
        balance!.AmountMinorUnits.Should().Be(5000);
    }

    private async Task<AccountResponse> CreateAccountAsync(string name, long? openingBalanceMinorUnits = null)
    {
        var response = await _client.PostAsJsonAsync("/api/accounts", new { name, type = "Cash", currency = "EGP", openingBalanceMinorUnits });
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        return (await response.Content.ReadFromJsonAsync<AccountResponse>())!;
    }

    private sealed record AccountResponse(Guid Id);
    private sealed record AccountLifecycleResponse(Guid Id, string Status);
    private sealed record BalanceResponse(long AmountMinorUnits, string Status);
    private sealed record OperationResponse(Guid Id, string Type, Guid? OriginalOperationId, IReadOnlyList<EffectResponse> Effects, IReadOnlyList<RecoverableEffectResponse> RecoverableEffects);
    private sealed record EffectResponse(Guid Id, Guid AccountId, long AmountMinorUnits, string Currency, string Direction, long Order);
    private sealed record RecoverableEffectResponse(Guid Id, Guid RecoverableId, long AmountMinorUnits, string Currency, string Direction, string? CounterpartyName, long Order);
    private sealed record RecoverableResponse(Guid RecoverableId, string? CounterpartyName, string Currency, long OutstandingMinorUnits, string Status, IReadOnlyList<RecoverableEffectResponse> Effects);
}
