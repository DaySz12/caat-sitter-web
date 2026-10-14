using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CatSitter.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddPaymentSlipFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "payment_rejection_reason",
                table: "bookings",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "payment_slip_uploaded_at",
                table: "bookings",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "payment_slip_url",
                table: "bookings",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "payment_rejection_reason",
                table: "bookings");

            migrationBuilder.DropColumn(
                name: "payment_slip_uploaded_at",
                table: "bookings");

            migrationBuilder.DropColumn(
                name: "payment_slip_url",
                table: "bookings");
        }
    }
}
