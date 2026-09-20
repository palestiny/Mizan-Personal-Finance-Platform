using Microsoft.EntityFrameworkCore;
using Mizan.Application.Finance;
using Mizan.Domain.Finance;
using Mizan.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);
builder.Services.ConfigureHttpJsonOptions(options => options.SerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter()));

builder.Services.AddDbContext<MizanDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Mizan") ?? "Host=localhost;Database=mizan;Username=postgres;Password=postgres"));
builder.Services.AddScoped<IFinanceRepository, EfFinanceRepository>();
builder.Services.AddScoped<FinanceService>();

var app = builder.Build();

app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        var exception = context.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerFeature>()?.Error;
        context.Response.StatusCode = exception switch
        {
            IdempotencyConflictException => StatusCodes.Status409Conflict,
            DomainValidationException => StatusCodes.Status400BadRequest,
            _ => StatusCodes.Status500InternalServerError
        };
        await Results.Problem(
            statusCode: context.Response.StatusCode,
            title: context.Response.StatusCode == StatusCodes.Status409Conflict ? "Idempotency conflict" :
                   context.Response.StatusCode == StatusCodes.Status400BadRequest ? "Validation error" :
                   "Internal server error").ExecuteAsync(context);
    });
});

if (app.Environment.IsEnvironment("Testing"))
{
    using var scope = app.Services.CreateScope();
    await scope.ServiceProvider.GetRequiredService<MizanDbContext>().Database.MigrateAsync();
}

app.MapGet("/", () => Results.Ok(new { service = "Mizan", status = "ok" }));

app.MapPost("/api/accounts", async (CreateAccountRequest request, FinanceService service, CancellationToken ct) =>
{
    var account = await service.CreateAccountAsync(
        request.Name, request.Type, request.Currency,
        request.OpeningBalanceMinorUnits is null ? null : Money.FromMinorUnits(request.OpeningBalanceMinorUnits.Value, request.Currency),
        ct);
    return Results.Created($"/api/accounts/{account.Id}", new { account.Id, account.Name, account.Type, account.Currency, account.Status });
});

app.MapPost("/api/accounts/{accountId:guid}/close", async (Guid accountId, FinanceService service, CancellationToken ct) =>
{
    var account = await service.CloseAccountAsync(new CloseAccountCommand(accountId), ct);
    return Results.Ok(new { account.Id, account.Name, account.Type, account.Currency, account.Status });
});

app.MapPost("/api/accounts/{accountId:guid}/reopen", async (Guid accountId, FinanceService service, CancellationToken ct) =>
{
    var account = await service.ReopenAccountAsync(new ReopenAccountCommand(accountId), ct);
    return Results.Ok(new { account.Id, account.Name, account.Type, account.Currency, account.Status });
});

app.MapPost("/api/operations/income", async (AcceptIncomeRequest request, FinanceService service, CancellationToken ct) =>
{
    var op = await service.AcceptAsync(
        FinancialOperation.Income(request.AccountId, Money.FromMinorUnits(request.AmountMinorUnits, request.Currency), request.EffectiveAt),
        request.IdempotencyKey, ct);
    return Results.Ok(OperationResponse.From(op));
});

app.MapPost("/api/operations/expense", async (AcceptExpenseRequest request, FinanceService service, CancellationToken ct) =>
{
    var op = await service.AcceptAsync(
        FinancialOperation.PersonalExpense(request.AccountId, Money.FromMinorUnits(request.AmountMinorUnits, request.Currency), request.EffectiveAt),
        request.IdempotencyKey, ct);
    return Results.Ok(OperationResponse.From(op));
});

app.MapPost("/api/operations/recoverable-expense", async (RecoverableExpenseRequest request, FinanceService service, CancellationToken ct) =>
{
    var op = await service.AcceptRecoverableExpenseAsync(
        new AcceptRecoverableExpenseCommand(
            request.AccountId,
            new MoneyInput(request.AmountMinorUnits, request.Currency),
            request.CounterpartyName,
            request.EffectiveAt.ToString("O"),
            request.IdempotencyKey),
        ct);
    return Results.Ok(OperationResponse.From(op));
});

app.MapPost("/api/operations/recoverable-settlement", async (RecoverableSettlementRequest request, FinanceService service, CancellationToken ct) =>
{
    var op = await service.AcceptRecoverableSettlementAsync(
        new SettleRecoverableCommand(
            request.AccountId,
            request.RecoverableId,
            new MoneyInput(request.AmountMinorUnits, request.Currency),
            request.EffectiveAt.ToString("O"),
            request.IdempotencyKey),
        ct);
    return Results.Ok(OperationResponse.From(op));
});

app.MapPost("/api/operations/transfer", async (TransferRequest request, FinanceService service, CancellationToken ct) =>
{
    var op = await service.AcceptAsync(
        FinancialOperation.OwnedAccountTransfer(request.SourceAccountId, request.DestinationAccountId, Money.FromMinorUnits(request.AmountMinorUnits, request.Currency), request.EffectiveAt),
        request.IdempotencyKey, ct);
    return Results.Ok(OperationResponse.From(op));
});

app.MapPost("/api/operations/reversal", async (ReverseOperationRequest request, FinanceService service, CancellationToken ct) =>
{
    var op = await service.AcceptReversalAsync(
        new ReverseOperationCommand(request.OriginalOperationId, request.EffectiveAt.ToString("O"), request.IdempotencyKey), ct);
    return Results.Ok(OperationResponse.From(op));
});

app.MapGet("/api/recoverables/{recoverableId:guid}", async (Guid recoverableId, FinanceService service, CancellationToken ct) =>
{
    var effects = await service.GetRecoverableEffectsAsync(recoverableId, ct);
    if (effects.Count == 0) return Results.NotFound();

    var balance = effects.Sum(x => x.SignedMinorUnits);
    var first = effects.FirstOrDefault(x => x.CounterpartyName is not null);
    return Results.Ok(new
    {
        recoverableId,
        counterpartyName = first?.CounterpartyName,
        currency = effects[0].Amount.Currency,
        outstandingMinorUnits = balance,
        status = balance == 0 ? "Settled" : "Outstanding",
        effects
    });
});

app.MapGet("/api/accounts/{accountId:guid}/balance", async (Guid accountId, IFinanceRepository repository, CancellationToken ct) =>
{
    var account = await repository.GetAccountAsync(accountId, ct);
    if (account is null) return Results.NotFound();

    var effects = await repository.GetEffectsAsync(accountId, ct);
    var balance = Balance.Derive(account, effects);
    return Results.Ok(new { accountId, status = account.Status, currency = balance.Currency, AmountMinorUnits = balance.MinorUnits });
});

app.MapGet("/api/accounts/{accountId:guid}/history", async (Guid accountId, IFinanceRepository repository, CancellationToken ct) =>
{
    var account = await repository.GetAccountAsync(accountId, ct);
    if (account is null) return Results.NotFound();

    var effects = await repository.GetEffectsAsync(accountId, ct);
    return Results.Ok(effects);
});

app.MapGet("/api/accounts/{accountId:guid}/balance/explanation", async (Guid accountId, IFinanceRepository repository, CancellationToken ct) =>
{
    var account = await repository.GetAccountAsync(accountId, ct);
    if (account is null) return Results.NotFound();

    var effects = await repository.GetEffectsAsync(accountId, ct);
    var balance = Balance.Derive(account, effects);
    return Results.Ok(new
    {
        accountId,
        currency = balance.Currency,
        minorUnits = balance.MinorUnits,
        explanation = effects.Select(x => new { x.Id, x.OperationId, x.Direction, x.Amount.MinorUnits, x.EffectiveAt, x.RecordedAt, x.Order }).ToArray()
    });
});

app.Run();

public partial class Program { }

public sealed record CreateAccountRequest(string Name, AccountType Type, string Currency, long? OpeningBalanceMinorUnits);
public sealed record AcceptIncomeRequest(Guid AccountId, long AmountMinorUnits, string Currency, DateTimeOffset EffectiveAt, string IdempotencyKey);
public sealed record AcceptExpenseRequest(Guid AccountId, long AmountMinorUnits, string Currency, DateTimeOffset EffectiveAt, string IdempotencyKey);
public sealed record RecoverableExpenseRequest(Guid AccountId, long AmountMinorUnits, string Currency, string CounterpartyName, DateTimeOffset EffectiveAt, string IdempotencyKey);
public sealed record RecoverableSettlementRequest(Guid AccountId, Guid RecoverableId, long AmountMinorUnits, string Currency, DateTimeOffset EffectiveAt, string IdempotencyKey);
public sealed record TransferRequest(Guid SourceAccountId, Guid DestinationAccountId, long AmountMinorUnits, string Currency, DateTimeOffset EffectiveAt, string IdempotencyKey);
public sealed record ReverseOperationRequest(Guid OriginalOperationId, DateTimeOffset EffectiveAt, string IdempotencyKey);

public sealed record OperationResponse(Guid Id, FinancialOperationType Type, DateTimeOffset EffectiveAt, DateTimeOffset RecordedAt, Guid? OriginalOperationId, IReadOnlyList<EffectResponse> Effects, IReadOnlyList<RecoverableEffectResponse> RecoverableEffects)
{
    public static OperationResponse From(FinancialOperation operation) =>
        new(operation.Id, operation.Type, operation.EffectiveAt, operation.RecordedAt, operation.OriginalOperationId,
            operation.Effects.Select(e => new EffectResponse(e.Id, e.AccountId, e.Amount.MinorUnits, e.Amount.Currency, e.Direction, e.Order)).ToArray(),
            operation.RecoverableEffects.Select(e => new RecoverableEffectResponse(e.Id, e.RecoverableId, e.Amount.MinorUnits, e.Amount.Currency, e.Direction, e.CounterpartyName, e.Order)).ToArray());
}

public sealed record EffectResponse(Guid Id, Guid AccountId, long AmountMinorUnits, string Currency, EffectDirection Direction, long Order);
public sealed record RecoverableEffectResponse(Guid Id, Guid RecoverableId, long AmountMinorUnits, string Currency, RecoverableEffectDirection Direction, string? CounterpartyName, long Order);
