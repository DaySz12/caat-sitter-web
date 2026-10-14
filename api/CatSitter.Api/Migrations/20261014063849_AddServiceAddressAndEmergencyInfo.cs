using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CatSitter.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddServiceAddressAndEmergencyInfo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "emergency_contact_phone",
                table: "cats",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "emergency_vet_clinic",
                table: "cats",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "emergency_contact_phone",
                table: "bookings",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "preferred_vet_clinic",
                table: "bookings",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "refund_bank_info",
                table: "bookings",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "refund_processed_at",
                table: "bookings",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "refund_reason",
                table: "bookings",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "refund_requested_at",
                table: "bookings",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "refund_status",
                table: "bookings",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "service_address",
                table: "bookings",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "emergency_contact_phone",
                table: "cats");

            migrationBuilder.DropColumn(
                name: "emergency_vet_clinic",
                table: "cats");

            migrationBuilder.DropColumn(
                name: "emergency_contact_phone",
                table: "bookings");

            migrationBuilder.DropColumn(
                name: "preferred_vet_clinic",
                table: "bookings");

            migrationBuilder.DropColumn(
                name: "refund_bank_info",
                table: "bookings");

            migrationBuilder.DropColumn(
                name: "refund_processed_at",
                table: "bookings");

            migrationBuilder.DropColumn(
                name: "refund_reason",
                table: "bookings");

            migrationBuilder.DropColumn(
                name: "refund_requested_at",
                table: "bookings");

            migrationBuilder.DropColumn(
                name: "refund_status",
                table: "bookings");

            migrationBuilder.DropColumn(
                name: "service_address",
                table: "bookings");
        }
    }
}
