using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CatSitter.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddBannersTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "banners",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    type = table.Column<string>(type: "text", nullable: false),
                    title = table.Column<string>(type: "text", nullable: false),
                    image_url = table.Column<string>(type: "text", nullable: false),
                    link_url = table.Column<string>(type: "text", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    display_order = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_banners", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_banners_display_order",
                table: "banners",
                column: "display_order");

            migrationBuilder.CreateIndex(
                name: "ix_banners_is_active",
                table: "banners",
                column: "is_active");

            migrationBuilder.CreateIndex(
                name: "ix_banners_type",
                table: "banners",
                column: "type");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "banners");
        }
    }
}
