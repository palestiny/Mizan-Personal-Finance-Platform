using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mizan.Infrastructure.Migrations
{
    public partial class AddReversalLinkage : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "OriginalOperationId",
                table: "financial_operations",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_financial_operations_OriginalOperationId",
                table: "financial_operations",
                column: "OriginalOperationId",
                unique: true,
                filter: "\"original_operation_id\" IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_financial_operations_financial_operations_OriginalOperationId",
                table: "financial_operations",
                column: "OriginalOperationId",
                principalTable: "financial_operations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_financial_operations_financial_operations_OriginalOperationId",
                table: "financial_operations");

            migrationBuilder.DropIndex(
                name: "IX_financial_operations_OriginalOperationId",
                table: "financial_operations");

            migrationBuilder.DropColumn(
                name: "OriginalOperationId",
                table: "financial_operations");
        }
    }
}
