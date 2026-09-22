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
    public async Task Api_should_serialize_account_close_against_concurrent_financial_operation()
    {
        var account = await CreateAccountAsync("Lifecycle Race", 5000);

        var responses = await Task.WhenAll(
            _client.PostAsync($"/api/accounts/{account.Id}/close", null),
            _client.PostAsJsonAsync("/api/operations/income", new
            {
                accountId = account.Id,
                amountMinorUnits = 100,
                currency = "EGP",
                effectiveAt = "2026-09-21T10:00:00+03:00",
                idempotencyKey = "lifecycle-race-income"
            }));

        responses.Select(x => x.StatusCode).Should().Contain(HttpStatusCode.OK);
        responses.Select(x => x.StatusCode).Should().Contain(HttpStatusCode.BadRequest);

        var balance = await _client.GetFromJsonAsync<BalanceResponse>($"/api/accounts/{account.Id}/balance");
        balance!.AmountMinorUnits.Should().Be(5000);
        balance.Status.Should().Be("Closed");
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
    public async Task Api_should_prevent_concurrent_over_settlement()
    {
        var account = await CreateAccountAsync("Recoverable Concurrent", 5000);
        var created = await _client.PostAsJsonAsync("/api/operations/recoverable-expense", new
        {
            accountId = account.Id, amountMinorUnits = 1000, currency = "EGP",
            counterpartyName = "Ahmed", effectiveAt = "2026-09-21T10:00:00+03:00", idempotencyKey = "recoverable-concurrent-expense"
        });
        var original = await created.Content.ReadFromJsonAsync<OperationResponse>();
        var recoverableId = original!.RecoverableEffects[0].RecoverableId;

        var requests = Enumerable.Range(1, 2).Select(i => _client.PostAsJsonAsync("/api/operations/recoverable-settlement", new
        {
            accountId = account.Id, recoverableId, amountMinorUnits = 600, currency = "EGP",
            effectiveAt = $"2026-09-21T10:0{i}:00+03:00", idempotencyKey = $"recoverable-concurrent-settlement-{i}"
        }));

        var responses = await Task.WhenAll(requests);

        responses.Select(x => x.StatusCode).Should().Contain(HttpStatusCode.OK);
        responses.Select(x => x.StatusCode).Should().Contain(HttpStatusCode.BadRequest);

        var recoverable = await _client.GetFromJsonAsync<RecoverableResponse>($"/api/recoverables/{recoverableId}");
        recoverable!.OutstandingMinorUnits.Should().Be(400);
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

    [Fact]
    public async Task Api_should_create_shared_expense_with_total_account_decrease_and_partial_recoverable()
    {
        var account = await CreateAccountAsync("Shared Expense Cash", 5000);

        var created = await _client.PostAsJsonAsync("/api/operations/shared-expense", new
        {
            accountId = account.Id,
            totalAmountMinorUnits = 1000,
            recoverableAmountMinorUnits = 400,
            currency = "EGP",
            counterpartyName = "Ahmed",
            effectiveAt = "2026-09-21T10:00:00+03:00",
            idempotencyKey = "shared-expense-1"
        });

        created.StatusCode.Should().Be(HttpStatusCode.OK);
        var operation = await created.Content.ReadFromJsonAsync<OperationResponse>();
        operation!.Type.Should().Be("SharedExpense");
        operation.Effects.Should().ContainSingle();
        operation.Effects[0].AmountMinorUnits.Should().Be(1000);
        operation.Effects[0].Direction.Should().Be("Decrease");
        operation.RecoverableEffects.Should().ContainSingle();
        operation.RecoverableEffects[0].AmountMinorUnits.Should().Be(400);

        var balance = await _client.GetFromJsonAsync<BalanceResponse>($"/api/accounts/{account.Id}/balance");
        balance!.AmountMinorUnits.Should().Be(4000);

        var recoverable = await _client.GetFromJsonAsync<RecoverableResponse>($"/api/recoverables/{operation.RecoverableEffects[0].RecoverableId}");
        recoverable!.OutstandingMinorUnits.Should().Be(400);
        recoverable.Status.Should().Be("Outstanding");
    }

    [Fact]
    public async Task Api_should_return_same_shared_expense_for_idempotent_retry()
    {
        var account = await CreateAccountAsync("Shared Expense Retry", 5000);

        var request = new
        {
            accountId = account.Id,
            totalAmountMinorUnits = 1000,
            recoverableAmountMinorUnits = 400,
            currency = "EGP",
            counterpartyName = "Ahmed",
            effectiveAt = "2026-09-21T10:00:00+03:00",
            idempotencyKey = "shared-expense-retry-1"
        };

        var first = await _client.PostAsJsonAsync("/api/operations/shared-expense", request);
        var second = await _client.PostAsJsonAsync("/api/operations/shared-expense", request);

        first.StatusCode.Should().Be(HttpStatusCode.OK);
        second.StatusCode.Should().Be(HttpStatusCode.OK);

        var firstOperation = await first.Content.ReadFromJsonAsync<OperationResponse>();
        var secondOperation = await second.Content.ReadFromJsonAsync<OperationResponse>();

        secondOperation!.Id.Should().Be(firstOperation!.Id);
        secondOperation.RecoverableEffects.Should().ContainSingle();
        secondOperation.RecoverableEffects[0].RecoverableId.Should().Be(firstOperation.RecoverableEffects[0].RecoverableId);

        var balance = await _client.GetFromJsonAsync<BalanceResponse>($"/api/accounts/{account.Id}/balance");
        balance!.AmountMinorUnits.Should().Be(4000);
    }

    [Fact]
    public async Task Api_should_reject_invalid_shared_expense_recoverable_portion()
    {
        var account = await CreateAccountAsync("Shared Expense Invalid");

        var zero = await _client.PostAsJsonAsync("/api/operations/shared-expense", new
        {
            accountId = account.Id,
            totalAmountMinorUnits = 1000,
            recoverableAmountMinorUnits = 0,
            currency = "EGP",
            counterpartyName = "Ahmed",
            effectiveAt = "2026-09-21T10:00:00+03:00",
            idempotencyKey = "shared-expense-zero"
        });
        zero.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var aboveTotal = await _client.PostAsJsonAsync("/api/operations/shared-expense", new
        {
            accountId = account.Id,
            totalAmountMinorUnits = 1000,
            recoverableAmountMinorUnits = 1001,
            currency = "EGP",
            counterpartyName = "Ahmed",
            effectiveAt = "2026-09-21T10:00:00+03:00",
            idempotencyKey = "shared-expense-above"
        });
        aboveTotal.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Api_should_reject_shared_expense_reversal_while_recoverable_is_settled()
    {
        var account = await CreateAccountAsync("Shared Expense Correction", 5000);

        var created = await _client.PostAsJsonAsync("/api/operations/shared-expense", new
        {
            accountId = account.Id,
            totalAmountMinorUnits = 1000,
            recoverableAmountMinorUnits = 400,
            currency = "EGP",
            counterpartyName = "Ahmed",
            effectiveAt = "2026-09-21T10:00:00+03:00",
            idempotencyKey = "shared-expense-correction-1"
        });
        var original = await created.Content.ReadFromJsonAsync<OperationResponse>();
        var recoverableId = original!.RecoverableEffects[0].RecoverableId;

        var settlement = await _client.PostAsJsonAsync("/api/operations/recoverable-settlement", new
        {
            accountId = account.Id,
            recoverableId,
            amountMinorUnits = 200,
            currency = "EGP",
            effectiveAt = "2026-09-22T10:00:00+03:00",
            idempotencyKey = "shared-expense-settlement-1"
        });
        settlement.StatusCode.Should().Be(HttpStatusCode.OK);

        var beforeReversal = await _client.GetFromJsonAsync<RecoverableResponse>($"/api/recoverables/{recoverableId}");
        beforeReversal!.OutstandingMinorUnits.Should().Be(200);

        var reversal = await _client.PostAsJsonAsync("/api/operations/reversal", new
        {
            originalOperationId = original.Id,
            effectiveAt = "2026-09-23T10:00:00+03:00",
            idempotencyKey = "shared-expense-reversal-1"
        });
        reversal.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var afterRejectedReversal = await _client.GetFromJsonAsync<RecoverableResponse>($"/api/recoverables/{recoverableId}");
        afterRejectedReversal!.OutstandingMinorUnits.Should().Be(200);
        afterRejectedReversal.Status.Should().Be("Outstanding");

        var balance = await _client.GetFromJsonAsync<BalanceResponse>($"/api/accounts/{account.Id}/balance");
        balance!.AmountMinorUnits.Should().Be(4200);
    }

    [Fact]
    public async Task Api_should_reverse_settlement_before_reversing_shared_expense()
    {
        var account = await CreateAccountAsync("Shared Expense Full Correction", 5000);

        var created = await _client.PostAsJsonAsync("/api/operations/shared-expense", new
        {
            accountId = account.Id,
            totalAmountMinorUnits = 1000,
            recoverableAmountMinorUnits = 400,
            currency = "EGP",
            counterpartyName = "Ahmed",
            effectiveAt = "2026-09-21T10:00:00+03:00",
            idempotencyKey = "shared-expense-full-correction-1"
        });
        created.StatusCode.Should().Be(HttpStatusCode.OK);
        var original = await created.Content.ReadFromJsonAsync<OperationResponse>();
        var recoverableId = original!.RecoverableEffects[0].RecoverableId;

        var settlement = await _client.PostAsJsonAsync("/api/operations/recoverable-settlement", new
        {
            accountId = account.Id,
            recoverableId,
            amountMinorUnits = 400,
            currency = "EGP",
            effectiveAt = "2026-09-22T10:00:00+03:00",
            idempotencyKey = "shared-expense-full-correction-2"
        });
        settlement.StatusCode.Should().Be(HttpStatusCode.OK);
        var settlementOperation = await settlement.Content.ReadFromJsonAsync<OperationResponse>();

        var directReversal = await _client.PostAsJsonAsync("/api/operations/reversal", new
        {
            originalOperationId = original.Id,
            effectiveAt = "2026-09-23T10:00:00+03:00",
            idempotencyKey = "shared-expense-full-correction-3"
        });
        directReversal.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var reverseSettlement = await _client.PostAsJsonAsync("/api/operations/reversal", new
        {
            originalOperationId = settlementOperation!.Id,
            effectiveAt = "2026-09-24T10:00:00+03:00",
            idempotencyKey = "shared-expense-full-correction-4"
        });
        reverseSettlement.StatusCode.Should().Be(HttpStatusCode.OK);

        var reverseOriginal = await _client.PostAsJsonAsync("/api/operations/reversal", new
        {
            originalOperationId = original.Id,
            effectiveAt = "2026-09-25T10:00:00+03:00",
            idempotencyKey = "shared-expense-full-correction-5"
        });
        reverseOriginal.StatusCode.Should().Be(HttpStatusCode.OK);

        var recoverable = await _client.GetFromJsonAsync<RecoverableResponse>($"/api/recoverables/{recoverableId}");
        recoverable!.OutstandingMinorUnits.Should().Be(0);
        recoverable.Status.Should().Be("Settled");

        var balance = await _client.GetFromJsonAsync<BalanceResponse>($"/api/accounts/{account.Id}/balance");
        balance!.AmountMinorUnits.Should().Be(5000);
    }


    [Fact]
    public async Task Api_should_create_obligation_without_changing_account_balance()
    {
        var account = await CreateAccountAsync("Obligation Cash", 5000);

        var created = await _client.PostAsJsonAsync("/api/obligations", new
        {
            description = "Rent",
            amountMinorUnits = 2000,
            currency = "EGP",
            dueAt = "2026-10-01T00:00:00+03:00"
        });

        created.StatusCode.Should().Be(HttpStatusCode.Created);
        var obligation = await created.Content.ReadFromJsonAsync<ObligationResponse>();
        obligation!.Status.Should().Be("Planned");
        obligation.AmountMinorUnits.Should().Be(2000);
        obligation.Currency.Should().Be("EGP");

        var balance = await _client.GetFromJsonAsync<BalanceResponse>($"/api/accounts/{account.Id}/balance");
        balance!.AmountMinorUnits.Should().Be(5000);
    }

    [Fact]
    public async Task Api_should_settle_and_cancel_obligations_without_creating_financial_effects()
    {
        var settled = await _client.PostAsJsonAsync("/api/obligations", new
        {
            description = "Utilities",
            amountMinorUnits = 700,
            currency = "EGP",
            dueAt = "2026-10-05T00:00:00+03:00"
        });
        var settledObligation = await settled.Content.ReadFromJsonAsync<ObligationResponse>();

        var settle = await _client.PostAsync($"/api/obligations/{settledObligation!.Id}/settle", null);
        settle.StatusCode.Should().Be(HttpStatusCode.OK);
        (await settle.Content.ReadFromJsonAsync<ObligationResponse>())!.Status.Should().Be("Settled");

        var cancelled = await _client.PostAsJsonAsync("/api/obligations", new
        {
            description = "Subscription",
            amountMinorUnits = 300,
            currency = "EGP",
            dueAt = "2026-11-01T00:00:00+03:00"
        });
        var cancelledObligation = await cancelled.Content.ReadFromJsonAsync<ObligationResponse>();

        var cancel = await _client.PostAsync($"/api/obligations/{cancelledObligation!.Id}/cancel", null);
        cancel.StatusCode.Should().Be(HttpStatusCode.OK);
        (await cancel.Content.ReadFromJsonAsync<ObligationResponse>())!.Status.Should().Be("Cancelled");
    }

    [Fact]
    public async Task Api_should_allow_only_one_concurrent_obligation_settlement()
    {
        var created = await _client.PostAsJsonAsync("/api/obligations", new
        {
            description = "Concurrent Rent",
            amountMinorUnits = 2000,
            currency = "EGP",
            dueAt = "2026-10-01T00:00:00+03:00"
        });
        created.StatusCode.Should().Be(HttpStatusCode.Created);
        var obligation = await created.Content.ReadFromJsonAsync<ObligationResponse>();

        var responses = await Task.WhenAll(
            _client.PostAsync($"/api/obligations/{obligation!.Id}/settle", null),
            _client.PostAsync($"/api/obligations/{obligation.Id}/settle", null));

        responses.Select(x => x.StatusCode).Should().Contain(HttpStatusCode.OK);
        responses.Select(x => x.StatusCode).Should().Contain(HttpStatusCode.BadRequest);

        var final = await _client.GetFromJsonAsync<ObligationResponse>($"/api/obligations/{obligation.Id}");
        final!.Status.Should().Be("Settled");
    }

    [Fact]
    public async Task Api_should_reject_duplicate_obligation_lifecycle_transition()
    {
        var created = await _client.PostAsJsonAsync("/api/obligations", new
        {
            description = "Rent",
            amountMinorUnits = 2000,
            currency = "EGP",
            dueAt = "2026-10-01T00:00:00+03:00"
        });
        var obligation = await created.Content.ReadFromJsonAsync<ObligationResponse>();

        (await _client.PostAsync($"/api/obligations/{obligation!.Id}/settle", null)).StatusCode.Should().Be(HttpStatusCode.OK);
        (await _client.PostAsync($"/api/obligations/{obligation.Id}/settle", null)).StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    private sealed record ObligationResponse(Guid Id, string Description, long AmountMinorUnits, string Currency, DateTimeOffset DueAt, DateTimeOffset CreatedAt, string Status);

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
