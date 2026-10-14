using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CatSitter.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddPayoutsAndPaymentSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "payout_requests",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    sitter_id = table.Column<Guid>(type: "uuid", nullable: false),
                    amount = table.Column<decimal>(type: "numeric(12,2)", nullable: false),
                    bank_name = table.Column<string>(type: "text", nullable: false),
                    bank_account_number = table.Column<string>(type: "text", nullable: false),
                    bank_account_name = table.Column<string>(type: "text", nullable: false),
                    prompt_pay_number = table.Column<string>(type: "text", nullable: true),
                    status = table.Column<string>(type: "text", nullable: false),
                    admin_notes = table.Column<string>(type: "text", nullable: true),
                    transfer_slip_url = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    processed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_payout_requests", x => x.id);
                    table.ForeignKey(
                        name: "fk_payout_requests_users_sitter_id",
                        column: x => x.sitter_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "platform_payment_settings",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    bank_name = table.Column<string>(type: "text", nullable: false),
                    bank_account_number = table.Column<string>(type: "text", nullable: false),
                    bank_account_name = table.Column<string>(type: "text", nullable: false),
                    prompt_pay_number = table.Column<string>(type: "text", nullable: false),
                    prompt_pay_name = table.Column<string>(type: "text", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_platform_payment_settings", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_payout_requests_created_at",
                table: "payout_requests",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "ix_payout_requests_sitter_id",
                table: "payout_requests",
                column: "sitter_id");

            migrationBuilder.CreateIndex(
                name: "ix_payout_requests_status",
                table: "payout_requests",
                column: "status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "payout_requests");

            migrationBuilder.DropTable(
                name: "platform_payment_settings");
        }
    }
}
