using Microsoft.EntityFrameworkCore;
using Mizan.Application.Finance;
using Mizan.Application.Capture;
using Mizan.Domain.Finance;
using Mizan.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);
builder.Services.ConfigureHttpJsonOptions(options => options.SerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter()));

builder.Services.AddDbContext<MizanDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Mizan") ?? "Host=localhost;Database=mizan;Username=postgres;Password=postgres"));
builder.Services.AddScoped<IFinanceRepository, EfFinanceRepository>();
builder.Services.AddScoped<FinanceService>();
builder.Services.AddScoped<IProposalRepository, EfProposalRepository>();
builder.Services.AddScoped<ProposalService>();

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

app.MapPost("/api/proposals", async (CreateProposalRequest request, ProposalService service, CancellationToken ct) =>
{
    var proposal = await service.CreateAsync(new CreateProposalCommand(
        request.OriginalInput, request.OperationType, request.AccountId, request.DestinationAccountId,
        request.AmountMinorUnits, request.Currency, request.EffectiveAt?.ToString("O"),
        request.MissingFields, request.Ambiguities, request.InterpretationMetadata, request.ExpiresAt.ToString("O")), ct);
    return Results.Created($"/api/proposals/{proposal.Id}", ProposalResponse.From(proposal));
});

app.MapGet("/api/proposals/{proposalId:guid}", async (Guid proposalId, ProposalService service, CancellationToken ct) =>
{
    var proposal = await service.GetAsync(proposalId, ct);
    return proposal is null ? Results.NotFound() : Results.Ok(ProposalResponse.From(proposal));
});

app.MapPost("/api/proposals/{proposalId:guid}/prepare", async (Guid proposalId, ProposalService service, CancellationToken ct) =>
{
    var proposal = await service.PrepareAsync(proposalId, ct);
    return Results.Ok(ProposalResponse.From(proposal));
});

app.MapPost("/api/proposals/{proposalId:guid}/reject", async (Guid proposalId, ProposalService service, CancellationToken ct) =>
{
    var proposal = await service.RejectAsync(new RejectProposalCommand(proposalId), ct);
    return Results.Ok(ProposalResponse.From(proposal));
});

app.MapPost("/api/proposals/{proposalId:guid}/expire", async (Guid proposalId, ProposalService service, CancellationToken ct) =>
{
    var proposal = await service.ExpireAsync(new ExpireProposalCommand(proposalId), ct);
    return Results.Ok(ProposalResponse.From(proposal));
});

app.MapPost("/api/proposals/{proposalId:guid}/confirm", async (Guid proposalId, ConfirmProposalRequest request, ProposalService service, CancellationToken ct) =>
{
    var operation = await service.ConfirmAsync(new ConfirmProposalCommand(proposalId, request.CommandIdempotencyKey), ct);
    return Results.Ok(OperationResponse.From(operation));
});




app.MapPost("/api/obligations", async (CreateObligationRequest request, FinanceService service, CancellationToken ct) =>
{
    var obligation = await service.CreateObligationAsync(
        new CreateObligationCommand(request.Description, new MoneyInput(request.AmountMinorUnits, request.Currency), request.DueAt.ToString("O")), ct);
    return Results.Created($"/api/obligations/{obligation.Id}", ObligationResponse.From(obligation));
});

app.MapPost("/api/obligations/{obligationId:guid}/settle", async (Guid obligationId, FinanceService service, CancellationToken ct) =>
{
    var obligation = await service.SettleObligationAsync(new SettleObligationCommand(obligationId), ct);
    return Results.Ok(ObligationResponse.From(obligation));
});

app.MapPost("/api/obligations/{obligationId:guid}/cancel", async (Guid obligationId, FinanceService service, CancellationToken ct) =>
{
    var obligation = await service.CancelObligationAsync(new CancelObligationCommand(obligationId), ct);
    return Results.Ok(ObligationResponse.From(obligation));
});

app.MapGet("/api/obligations/{obligationId:guid}", async (Guid obligationId, FinanceService service, CancellationToken ct) =>
{
    var obligation = await service.GetObligationAsync(obligationId, ct);
    return obligation is null ? Results.NotFound() : Results.Ok(ObligationResponse.From(obligation));
});

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

app.MapPost("/api/operations/shared-expense", async (SharedExpenseRequest request, FinanceService service, CancellationToken ct) =>
{
    var op = await service.AcceptSharedExpenseAsync(
        new AcceptSharedExpenseCommand(
            request.AccountId,
            new MoneyInput(request.TotalAmountMinorUnits, request.Currency),
            new MoneyInput(request.RecoverableAmountMinorUnits, request.Currency),
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


public sealed record CreateProposalRequest(string OriginalInput, ProposalOperationType OperationType, Guid? AccountId, Guid? DestinationAccountId, long? AmountMinorUnits, string? Currency, DateTimeOffset? EffectiveAt, string? MissingFields, string? Ambiguities, string? InterpretationMetadata, DateTimeOffset ExpiresAt);
public sealed record ConfirmProposalRequest(string CommandIdempotencyKey);
public sealed record ProposalResponse(Guid Id,string OriginalInput,ProposalOperationType OperationType,Guid? AccountId,Guid? DestinationAccountId,long? AmountMinorUnits,string? Currency,DateTimeOffset? EffectiveAt,string? MissingFields,string? Ambiguities,string? InterpretationMetadata,string Status,DateTimeOffset CreatedAt,DateTimeOffset ExpiresAt,DateTimeOffset? ConfirmedAt,DateTimeOffset? RejectedAt,string? CommandIdempotencyKey,Guid? OperationId)
{
    public static ProposalResponse From(Proposal p)=>new(p.Id,p.OriginalInput,p.OperationType,p.AccountId,p.DestinationAccountId,p.AmountMinorUnits,p.Currency,p.EffectiveAt,p.MissingFields,p.Ambiguities,p.InterpretationMetadata,p.Status.ToString(),p.CreatedAt,p.ExpiresAt,p.ConfirmedAt,p.RejectedAt,p.CommandIdempotencyKey,p.OperationId);
}

public sealed record CreateObligationRequest(string Description, long AmountMinorUnits, string Currency, DateTimeOffset DueAt);
public sealed record ObligationResponse(Guid Id, string Description, long AmountMinorUnits, string Currency, DateTimeOffset DueAt, DateTimeOffset CreatedAt, ObligationStatus Status)
{
    public static ObligationResponse From(Obligation o) => new(o.Id, o.Description, o.Amount.MinorUnits, o.Amount.Currency, o.DueAt, o.CreatedAt, o.Status);
}

public sealed record CreateAccountRequest(string Name, AccountType Type, string Currency, long? OpeningBalanceMinorUnits);
public sealed record AcceptIncomeRequest(Guid AccountId, long AmountMinorUnits, string Currency, DateTimeOffset EffectiveAt, string IdempotencyKey);
public sealed record AcceptExpenseRequest(Guid AccountId, long AmountMinorUnits, string Currency, DateTimeOffset EffectiveAt, string IdempotencyKey);
public sealed record RecoverableExpenseRequest(Guid AccountId, long AmountMinorUnits, string Currency, string CounterpartyName, DateTimeOffset EffectiveAt, string IdempotencyKey);
public sealed record SharedExpenseRequest(Guid AccountId, long TotalAmountMinorUnits, long RecoverableAmountMinorUnits, string Currency, string CounterpartyName, DateTimeOffset EffectiveAt, string IdempotencyKey);
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
