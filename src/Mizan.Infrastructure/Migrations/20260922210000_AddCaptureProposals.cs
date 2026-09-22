using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mizan.Infrastructure.Migrations;

public partial class AddCaptureProposals : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "capture_proposals",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                OriginalInput = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                OperationType = table.Column<int>(type: "integer", nullable: false),
                AccountId = table.Column<Guid>(type: "uuid", nullable: true),
                DestinationAccountId = table.Column<Guid>(type: "uuid", nullable: true),
                AmountMinorUnits = table.Column<long>(type: "bigint", nullable: true),
                Currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: true),
                EffectiveAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                MissingFields = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                Ambiguities = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                InterpretationMetadata = table.Column<string>(type: "character varying(8000)", maxLength: 8000, nullable: true),
                Status = table.Column<int>(type: "integer", nullable: false),
                CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                ExpiresAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                ConfirmedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                RejectedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                CommandIdempotencyKey = table.Column<string>(type: "text", nullable: true),
                OperationId = table.Column<Guid>(type: "uuid", nullable: true)
            },
            constraints: table => { table.PrimaryKey("PK_capture_proposals", x => x.Id); });

        migrationBuilder.CreateIndex(
            name: "IX_capture_proposals_CommandIdempotencyKey",
            table: "capture_proposals",
            column: "CommandIdempotencyKey",
            unique: true,
            filter: "\"CommandIdempotencyKey\" IS NOT NULL");

        migrationBuilder.CreateIndex(
            name: "IX_capture_proposals_Status_ExpiresAt",
            table: "capture_proposals",
            columns: new[] { "Status", "ExpiresAt" });
    }

    protected override void Down(MigrationBuilder migrationBuilder) => migrationBuilder.DropTable(name: "capture_proposals");
}
