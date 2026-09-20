using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mizan.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialFinancialCore : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "accounts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    Currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    OpeningBalanceMinorUnits = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table => table.PrimaryKey("PK_accounts", x => x.Id));

            migrationBuilder.CreateTable(
                name: "financial_operations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    EffectiveAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    RecordedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table => table.PrimaryKey("PK_financial_operations", x => x.Id));

            migrationBuilder.CreateTable(
                name: "idempotency_keys",
                columns: table => new
                {
                    Key = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false, collation: "C"),
                    OperationId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table => table.PrimaryKey("PK_idempotency_keys", x => x.Key));

            migrationBuilder.CreateTable(
                name: "financial_effects",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OperationId = table.Column<Guid>(type: "uuid", nullable: false),
                    AccountId = table.Column<Guid>(type: "uuid", nullable: false),
                    AmountMinorUnits = table.Column<long>(type: "bigint", nullable: false),
                    Direction = table.Column<int>(type: "integer", nullable: false),
                    Currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    EffectiveAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    RecordedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Order = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_financial_effects", x => x.Id);
                    table.ForeignKey(name: "FK_financial_effects_accounts_AccountId", column: x => x.AccountId, principalTable: "accounts", principalColumn: "Id", onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(name: "FK_financial_effects_financial_operations_OperationId", column: x => x.OperationId, principalTable: "financial_operations", principalColumn: "Id", onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(name: "IX_accounts_Name_Currency", table: "accounts", columns: new[] { "Name", "Currency" });
            migrationBuilder.CreateIndex(name: "IX_financial_effects_AccountId_EffectiveAt_RecordedAt_Order_Id", table: "financial_effects", columns: new[] { "AccountId", "EffectiveAt", "RecordedAt", "Order", "Id" });
            migrationBuilder.CreateIndex(name: "IX_financial_effects_OperationId", table: "financial_effects", column: "OperationId");
            migrationBuilder.CreateIndex(name: "IX_idempotency_keys_OperationId", table: "idempotency_keys", column: "OperationId", unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "financial_effects");
            migrationBuilder.DropTable(name: "idempotency_keys");
            migrationBuilder.DropTable(name: "accounts");
            migrationBuilder.DropTable(name: "financial_operations");
        }
    }
}