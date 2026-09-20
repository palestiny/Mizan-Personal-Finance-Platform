using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Mizan.Application.Finance;
using Mizan.Domain.Finance;

namespace Mizan.Infrastructure.Persistence;

public sealed class EfFinanceRepository : IFinanceRepository
{
    private readonly MizanDbContext _db;
    private IDbContextTransaction? _transaction;

    public EfFinanceRepository(MizanDbContext db) => _db = db;

    public Task<Account> AddAccountAsync(Account account, CancellationToken cancellationToken)
    {
        _db.Accounts.Add(new AccountRecord
        {
            Id = account.Id,
            Name = account.Name,
            Type = (int)account.Type,
            Currency = account.Currency,
            OpeningBalanceMinorUnits = account.OpeningBalance.MinorUnits,
            Status = (int)account.Status
        });
        return Task.FromResult(account);
    }

    public async Task<Account?> GetAccountAsync(Guid accountId, CancellationToken cancellationToken)
    {
        var r = await _db.Accounts.AsNoTracking().SingleOrDefaultAsync(x => x.Id == accountId, cancellationToken);
        return r is null ? null : Account.Rehydrate(r.Id, r.Name, (AccountType)r.Type, r.Currency, Money.FromMinorUnits(r.OpeningBalanceMinorUnits, r.Currency), (AccountStatus)r.Status);
    }

    public async Task<FinancialOperation?> GetOperationAsync(Guid operationId, CancellationToken cancellationToken)
    {
        var op = await _db.Operations.AsNoTracking().SingleOrDefaultAsync(x => x.Id == operationId, cancellationToken);
        if (op is null) return null;

        var effects = await _db.Effects.AsNoTracking()
            .Where(x => x.OperationId == op.Id)
            .OrderBy(x => x.Order)
            .ToListAsync(cancellationToken);
        return Rehydrate(op, effects, op.OriginalOperationId);
    }

    public async Task<FinancialOperation?> GetReversalByOriginalOperationIdAsync(Guid originalOperationId, CancellationToken cancellationToken)
    {
        var op = await _db.Operations.AsNoTracking()
            .SingleOrDefaultAsync(x => x.OriginalOperationId == originalOperationId, cancellationToken);
        if (op is null) return null;

        var effects = await _db.Effects.AsNoTracking()
            .Where(x => x.OperationId == op.Id)
            .OrderBy(x => x.Order)
            .ToListAsync(cancellationToken);
        return Rehydrate(op, effects, op.OriginalOperationId);
    }

    public async Task<FinancialOperation?> GetOperationByIdempotencyKeyAsync(string idempotencyKey, CancellationToken cancellationToken)
    {
        var key = await _db.IdempotencyKeys.AsNoTracking().SingleOrDefaultAsync(x => x.Key == idempotencyKey, cancellationToken);
        if (key is null) return null;

        var op = await _db.Operations.AsNoTracking().SingleAsync(x => x.Id == key.OperationId, cancellationToken);
        var effects = await _db.Effects.AsNoTracking().Where(x => x.OperationId == op.Id).OrderBy(x => x.Order).ToListAsync(cancellationToken);
        return Rehydrate(op, effects, op.OriginalOperationId);
    }

    public Task AddAcceptedOperationAsync(FinancialOperation operation, string idempotencyKey, CancellationToken cancellationToken)
    {
        _db.Operations.Add(new OperationRecord { Id = operation.Id, Type = (int)operation.Type, EffectiveAt = operation.EffectiveAt, RecordedAt = operation.RecordedAt, OriginalOperationId = operation.OriginalOperationId });
        foreach (var effect in operation.Effects)
            _db.Effects.Add(new EffectRecord
            {
                Id = effect.Id, OperationId = effect.OperationId, AccountId = effect.AccountId,
                AmountMinorUnits = effect.Amount.MinorUnits, Direction = (int)effect.Direction,
                Currency = effect.Amount.Currency, EffectiveAt = effect.EffectiveAt, RecordedAt = effect.RecordedAt, Order = effect.Order
            });
        _db.IdempotencyKeys.Add(new IdempotencyRecord { Key = idempotencyKey, OperationId = operation.Id });
        return Task.CompletedTask;
    }

    public async Task<IReadOnlyList<FinancialEffect>> GetEffectsAsync(Guid accountId, CancellationToken cancellationToken)
    {
        var rows = await _db.Effects.AsNoTracking().Where(x => x.AccountId == accountId)
            .OrderBy(x => x.EffectiveAt).ThenBy(x => x.RecordedAt).ThenBy(x => x.Order).ThenBy(x => x.Id).ToListAsync(cancellationToken);
        return rows.Select(ToDomain).ToArray();
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken) => _db.SaveChangesAsync(cancellationToken);

    public async Task BeginTransactionAsync(CancellationToken cancellationToken) =>
        _transaction = await _db.Database.BeginTransactionAsync(cancellationToken);

    public async Task CommitTransactionAsync(CancellationToken cancellationToken)
    {
        if (_transaction is not null) await _transaction.CommitAsync(cancellationToken);
    }

    public async Task RollbackTransactionAsync(CancellationToken cancellationToken)
    {
        if (_transaction is not null) await _transaction.RollbackAsync(cancellationToken);
    }

    public async Task UpdateAccountAsync(Account account, CancellationToken cancellationToken)
    {
        var row = await _db.Accounts.SingleAsync(x => x.Id == account.Id, cancellationToken);
        row.Status = (int)account.Status;
    }

    private static FinancialOperation Rehydrate(OperationRecord op, IReadOnlyList<EffectRecord> effects, Guid? originalOperationId) =>
        FinancialOperation.Rehydrate(op.Id, (FinancialOperationType)op.Type, op.EffectiveAt, op.RecordedAt, effects.Select(ToDomain).ToArray(), originalOperationId);

    private static FinancialEffect ToDomain(EffectRecord row) =>
        FinancialEffect.Rehydrate(row.Id, row.OperationId, row.AccountId, Money.FromMinorUnits(row.AmountMinorUnits, row.Currency),
            (EffectDirection)row.Direction, row.EffectiveAt, row.RecordedAt, row.Order);
}
