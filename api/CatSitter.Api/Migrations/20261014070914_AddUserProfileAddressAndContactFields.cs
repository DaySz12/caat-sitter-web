using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CatSitter.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddUserProfileAddressAndContactFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "address",
                table: "users",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "building_note",
                table: "users",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "emergency_contact_name",
                table: "users",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "latitude",
                table: "users",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "line_id",
                table: "users",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "longitude",
                table: "users",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "secondary_phone",
                table: "users",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "address",
                table: "users");

            migrationBuilder.DropColumn(
                name: "building_note",
                table: "users");

            migrationBuilder.DropColumn(
                name: "emergency_contact_name",
                table: "users");

            migrationBuilder.DropColumn(
                name: "latitude",
                table: "users");

            migrationBuilder.DropColumn(
                name: "line_id",
                table: "users");

            migrationBuilder.DropColumn(
                name: "longitude",
                table: "users");

            migrationBuilder.DropColumn(
                name: "secondary_phone",
                table: "users");
        }
    }
}
