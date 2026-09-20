using Microsoft.EntityFrameworkCore;

namespace Mizan.Infrastructure.Persistence;

public sealed class MizanDbContext(DbContextOptions<MizanDbContext> options) : DbContext(options)
{
    public DbSet<AccountRecord> Accounts => Set<AccountRecord>();
    public DbSet<OperationRecord> Operations => Set<OperationRecord>();
    public DbSet<EffectRecord> Effects => Set<EffectRecord>();
    public DbSet<RecoverableEffectRecord> RecoverableEffects => Set<RecoverableEffectRecord>();
    public DbSet<IdempotencyRecord> IdempotencyKeys => Set<IdempotencyRecord>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AccountRecord>(e =>
        {
            e.ToTable("accounts");
            e.HasKey(x => x.Id);
            e.Property(x => x.Name).HasMaxLength(200).IsRequired();
            e.Property(x => x.Currency).HasMaxLength(3).IsRequired();
            e.Property(x => x.OpeningBalanceMinorUnits).IsRequired();
            e.Property(x => x.Status).IsRequired();
            e.HasIndex(x => new { x.Name, x.Currency });
        });

        modelBuilder.Entity<OperationRecord>(e =>
        {
            e.ToTable("financial_operations");
            e.HasKey(x => x.Id);
            e.Property(x => x.Type).IsRequired();
            e.Property(x => x.EffectiveAt).IsRequired();
            e.Property(x => x.RecordedAt).IsRequired();
            e.Property(x => x.OriginalOperationId);
            e.HasOne<OperationRecord>().WithMany().HasForeignKey(x => x.OriginalOperationId).OnDelete(DeleteBehavior.Restrict);
            e.HasIndex(x => x.OriginalOperationId).IsUnique().HasFilter(""OriginalOperationId" IS NOT NULL");
        });

        modelBuilder.Entity<EffectRecord>(e =>
        {
            e.ToTable("financial_effects");
            e.HasKey(x => x.Id);
            e.HasIndex(x => new { x.AccountId, x.EffectiveAt, x.RecordedAt, x.Order, x.Id });
            e.HasIndex(x => x.OperationId);
            e.Property(x => x.Currency).HasMaxLength(3).IsRequired();
            e.Property(x => x.AmountMinorUnits).IsRequired();
            e.HasOne<OperationRecord>().WithMany().HasForeignKey(x => x.OperationId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne<AccountRecord>().WithMany().HasForeignKey(x => x.AccountId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<RecoverableEffectRecord>(e =>
        {
            e.ToTable("recoverable_effects");
            e.HasKey(x => x.Id);
            e.HasIndex(x => new { x.RecoverableId, x.EffectiveAt, x.RecordedAt, x.Order, x.Id });
            e.HasIndex(x => x.OperationId);
            e.Property(x => x.AmountMinorUnits).IsRequired();
            e.Property(x => x.Currency).HasMaxLength(3).IsRequired();
            e.Property(x => x.CounterpartyName).HasMaxLength(200);
            e.HasOne<OperationRecord>().WithMany().HasForeignKey(x => x.OperationId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<IdempotencyRecord>(e =>
        {
            e.ToTable("idempotency_keys");
            e.HasKey(x => x.Key);
            e.Property(x => x.Key).HasMaxLength(200);
            e.HasIndex(x => x.OperationId).IsUnique();
            e.Property(x => x.Key).UseCollation("C");
        });
    }
}

public sealed class AccountRecord
{
    public Guid Id { get; set; }
    public string Name { get; set; } = "";
    public int Type { get; set; }
    public string Currency { get; set; } = "";
    public long OpeningBalanceMinorUnits { get; set; }
    public int Status { get; set; }
}

public sealed class OperationRecord
{
    public Guid Id { get; set; }
    public int Type { get; set; }
    public DateTimeOffset EffectiveAt { get; set; }
    public DateTimeOffset RecordedAt { get; set; }
    public Guid? OriginalOperationId { get; set; }
}

public sealed class EffectRecord
{
    public Guid Id { get; set; }
    public Guid OperationId { get; set; }
    public Guid AccountId { get; set; }
    public long AmountMinorUnits { get; set; }
    public int Direction { get; set; }
    public string Currency { get; set; } = "";
    public DateTimeOffset EffectiveAt { get; set; }
    public DateTimeOffset RecordedAt { get; set; }
    public long Order { get; set; }
}

public sealed class RecoverableEffectRecord
{
    public Guid Id { get; set; }
    public Guid OperationId { get; set; }
    public Guid RecoverableId { get; set; }
    public long AmountMinorUnits { get; set; }
    public int Direction { get; set; }
    public string Currency { get; set; } = "";
    public string? CounterpartyName { get; set; }
    public DateTimeOffset EffectiveAt { get; set; }
    public DateTimeOffset RecordedAt { get; set; }
    public long Order { get; set; }
}

public sealed class IdempotencyRecord
{
    public string Key { get; set; } = "";
    public Guid OperationId { get; set; }
}
